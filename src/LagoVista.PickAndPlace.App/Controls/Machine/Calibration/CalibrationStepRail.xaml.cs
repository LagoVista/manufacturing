using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.App.Controls.Machine.Calibration
{
    /// <summary>
    /// Reusable 1-N procedure step rail. It observes the typed stage status
    /// rather than tying scroll behavior to any particular calibration runner.
    /// </summary>
    public partial class CalibrationStepRail : UserControl
    {
        private INotifyCollectionChanged _observedCollection;
        private readonly List<ICalibrationProcedureStep> _observedSteps =
            new List<ICalibrationProcedureStep>();

        public CalibrationStepRail()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty StepsProperty =
            DependencyProperty.Register(nameof(Steps), typeof(IEnumerable),
                typeof(CalibrationStepRail), new PropertyMetadata(null, StepsChanged));

        public IEnumerable Steps
        {
            get => (IEnumerable)GetValue(StepsProperty);
            set => SetValue(StepsProperty, value);
        }

        private static void StepsChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs args)
        {
            var rail = (CalibrationStepRail)dependencyObject;
            if (rail.IsLoaded)
                rail.RebindSteps();
        }

        private void StepRail_Loaded(object sender, RoutedEventArgs e)
        {
            RebindSteps();
        }

        private void StepRail_Unloaded(object sender, RoutedEventArgs e)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            if (_observedCollection != null)
                _observedCollection.CollectionChanged -= StepsCollectionChanged;
            _observedCollection = null;
            foreach (var step in _observedSteps)
                step.PropertyChanged -= StepPropertyChanged;
            _observedSteps.Clear();
        }

        private void RebindSteps()
        {
            Disconnect();
            _observedCollection = Steps as INotifyCollectionChanged;
            if (_observedCollection != null)
                _observedCollection.CollectionChanged += StepsCollectionChanged;

            if (Steps != null)
            {
                foreach (var step in Steps.OfType<ICalibrationProcedureStep>())
                {
                    _observedSteps.Add(step);
                    step.PropertyChanged += StepPropertyChanged;
                }
            }
            FollowActiveStep();
        }

        private void StepsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Replacing/resetting a run's steps must never leave old observers.
            RebindSteps();
        }

        private void StepPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ICalibrationProcedureStep.Status) &&
                !String.IsNullOrEmpty(e.PropertyName))
                return;

            var step = sender as ICalibrationProcedureStep;
            if (step?.Status == "Running" || step?.Status == "Failed")
                ScrollToStep(step);
        }

        private void FollowActiveStep()
        {
            var active = _observedSteps.FirstOrDefault(s => s.Status == "Running" ||
                                                           s.Status == "Failed");
            if (active != null)
                ScrollToStep(active);
        }

        private void ScrollToStep(ICalibrationProcedureStep step)
        {
            // The binding/ItemsControl layout needs to produce the new container
            // before BringIntoView. Dispatcher keeps this after the status update.
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                if (!IsLoaded || step == null || !ReferenceEquals(Steps, _observedCollection) &&
                    !_observedSteps.Contains(step))
                    return;

                StepItems.UpdateLayout();
                if (StepItems.ItemContainerGenerator.ContainerFromItem(step) is FrameworkElement container)
                    container.BringIntoView();
            }));
        }
    }
}
