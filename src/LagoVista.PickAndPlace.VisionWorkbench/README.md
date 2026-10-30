# Vision Workbench (first cut)

Standalone Windows WPF application for image-based vision experiments.

## Run

Open `LagoVista.PickAndPlace.VisionWorkbench.csproj` in Visual Studio 2022 with the .NET 8 desktop development workload, set it as startup, and press **F5**. Or from Windows: `dotnet run --project src/LagoVista.PickAndPlace.VisionWorkbench`.

Load a JPG/PNG via Open or drag/drop. Change the threshold, invert it, and save the processed PNG. The footer includes input clipping metrics.

## Intentional limitations

This first cut performs a simple managed grayscale/threshold baseline. It does **not** yet load VisionProfile, invoke Emgu CV or production detectors, control lights/cameras, or move hardware. It intentionally has zero dependencies on the production app. Those integrations follow after verifying that the small WPF shell launches and image pipeline works.

No modifications to existing application or solution files.
