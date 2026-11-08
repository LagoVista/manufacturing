using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Presentation-only stabilization for the live feeder barcode preview.
    /// The scanner must still receive the ORIGINAL per-frame binary signal.
    /// Records 30 raw edge measurements and uses a five-frame temporal
    /// binary median when the first bar remains in the same image location.
    /// </summary>
    public sealed class FeederBarcodeTemporalMonitor
    {
        private const int DiagnosticFrameCount = 30;
        private const int MedianFrameCount = 5;
        private const int MaximumEdgeMotionPixels = 7;
        private const int MaximumHoldFrames = 2;

        private sealed class EdgeFrame
        {
            public int Start { get; set; }
            public int End { get; set; }
            public bool RawVerified { get; set; }
        }

        private readonly Queue<EdgeFrame> _edges = new Queue<EdgeFrame>();
        private readonly Queue<byte[]> _signals = new Queue<byte[]>();
        private int? _anchorStart;
        private int? _anchorEnd;
        private int _frameLength;
        private byte _threshold;
        private FeederBarcodeCalibratedRead _lastVerifiedDisplay;
        private int _heldFrames;

        public int DiagnosticFrames => _edges.Count;
        public int MedianFrames => _signals.Count;

        public void Reset()
        {
            _edges.Clear();
            _signals.Clear();
            _anchorStart = null;
            _anchorEnd = null;
            _frameLength = 0;
            _threshold = 0;
            _lastVerifiedDisplay = null;
            _heldFrames = 0;
        }

        public FeederBarcodeTemporalFrame Observe(byte[] rawSignal,
            double pixelsPerMm, byte whiteThreshold, FeederBarcodeCalibratedRead raw)
        {
            if (rawSignal == null)
                throw new ArgumentNullException(nameof(rawSignal));
            if (raw == null)
                throw new ArgumentNullException(nameof(raw));

            // This tracker never pretends a newly moved camera still has the
            // old start bar. Movement, profile changes and loss of the leading
            // complete bar invalidate the temporal window immediately.
            var first = raw.Bars.Count >= 3 ? raw.Bars[0] : null;
            var hasCandidate = first != null && first.StartPixel > 0 &&
                first.StartPixel < rawSignal.Length / 2 &&
                first.EndPixel < rawSignal.Length;

            if (!hasCandidate)
            {
                Reset();
                return new FeederBarcodeTemporalFrame(raw, raw,
                    "RAW30: no complete START candidate", 0, false);
            }

            if (_anchorStart.HasValue &&
                (_frameLength != rawSignal.Length || _threshold != whiteThreshold ||
                 Math.Abs(_anchorStart.Value - first.StartPixel) > MaximumEdgeMotionPixels ||
                 Math.Abs(_anchorEnd.Value - first.EndPixel) > MaximumEdgeMotionPixels))
            {
                Reset();
            }

            if (!_anchorStart.HasValue)
            {
                _anchorStart = first.StartPixel;
                _anchorEnd = first.EndPixel;
                _frameLength = rawSignal.Length;
                _threshold = whiteThreshold;
            }

            _edges.Enqueue(new EdgeFrame
            {
                Start = first.StartPixel,
                End = first.EndPixel,
                RawVerified = raw.HasVerifiedStart
            });
            while (_edges.Count > DiagnosticFrameCount)
                _edges.Dequeue();

            _signals.Enqueue((byte[])rawSignal.Clone());
            while (_signals.Count > MedianFrameCount)
                _signals.Dequeue();

            var display = raw;
            var usingHold = false;
            // Use odd-sized median windows so ties can never toggle the result.
            // When the first five frames are accumulating, use the latest
            // three complete frames rather than a 4-frame majority tie.
            var windowSize = _signals.Count >= MedianFrameCount ? 5 :
                             _signals.Count >= 3 ? 3 : 0;
            if (windowSize >= 3)
            {
                var signals = _signals.Skip(_signals.Count - windowSize).ToArray();
                var filtered = new byte[rawSignal.Length];
                for (var pixel = 0; pixel < filtered.Length; pixel++)
                {
                    var brightCount = 0;
                    for (var frame = 0; frame < signals.Length; frame++)
                        if (signals[frame][pixel] >= whiteThreshold)
                            brightCount++;

                    filtered[pixel] = brightCount > windowSize / 2 ? (byte)255 : (byte)0;
                }

                display = FeederBarcodeSignal.ReadFromStart(filtered, pixelsPerMm, whiteThreshold);
            }

            if (display.HasVerifiedStart)
            {
                _lastVerifiedDisplay = display;
                _heldFrames = 0;
            }
            else if (_lastVerifiedDisplay != null && _heldFrames < MaximumHoldFrames)
            {
                // A short visual-only hold is explicitly marked on screen.
                // Nothing here changes the raw signal or the scanner's read.
                _heldFrames++;
                display = _lastVerifiedDisplay;
                usingHold = true;
            }
            else
            {
                _lastVerifiedDisplay = null;
                _heldFrames = 0;
            }

            var starts = _edges.Select(edge => edge.Start).ToArray();
            var ends = _edges.Select(edge => edge.End).ToArray();
            var widths = _edges.Select(edge => edge.End - edge.Start).ToArray();
            var rawOk = _edges.Count(edge => edge.RawVerified);

            var message = $"RAW30 {_edges.Count}/30 OK {rawOk}/{_edges.Count}  " +
                $"edges {starts.Min()}-{starts.Max()}/{ends.Min()}-{ends.Max()}px  " +
                $"width {widths.Min()}-{widths.Max()}px  M{windowSize}";
            if (usingHold)
                message += $" HOLD {_heldFrames}/{MaximumHoldFrames}";

            return new FeederBarcodeTemporalFrame(raw, display, message,
                windowSize, usingHold);
        }
    }

    public sealed class FeederBarcodeTemporalFrame
    {
        public FeederBarcodeCalibratedRead RawRead { get; }
        public FeederBarcodeCalibratedRead DisplayRead { get; }
        public string Diagnostics { get; }
        public int MedianWindowFrames { get; }
        public bool UsingHold { get; }

        public FeederBarcodeTemporalFrame(FeederBarcodeCalibratedRead rawRead,
            FeederBarcodeCalibratedRead displayRead, string diagnostics,
            int medianWindowFrames, bool usingHold)
        {
            RawRead = rawRead;
            DisplayRead = displayRead;
            Diagnostics = diagnostics;
            MedianWindowFrames = medianWindowFrames;
            UsingHold = usingHold;
        }
    }
}
