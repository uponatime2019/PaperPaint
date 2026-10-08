# Contributing to PaperPaint

Thank you for your interest in contributing to **PaperPaint**! Whether you are fixing a bug, adding new painting tools or coloring pages, improving performance, or enhancing the documentation, your contributions are warmly welcomed.

---

## 🛠️ Getting Started

### Prerequisites

To build and run PaperPaint locally from source:

1. **Operating System**: Windows 10 (version 1809+ / Build 17763+) or Windows 11.
2. **.NET SDK**: .NET 8.0 SDK (or later).
3. **IDE**:
   - [Visual Studio 2022](https://visualstudio.microsoft.com/) (version 17.8+ recommended) with the **.NET Desktop Development** workload and **Windows App SDK C# Templates**.
   - Or Visual Studio Code / JetBrains Rider with .NET 8 support.

### Clone and Build

1. Clone the repository:
   ```bash
   git clone https://github.com/uponatime2019/PaperPaint.git
   cd PaperPaint
   ```

2. Open the solution in Visual Studio:
   ```bash
   start PaperPaint.sln
   ```
   Or build from the command line:
   ```powershell
   dotnet build "PaperPaint.csproj" -p:Platform=x64
   ```

3. Run the application:
   ```powershell
   dotnet run --project "PaperPaint.csproj" -p:Platform=x64
   ```

---

## 🏗️ Project Architecture

PaperPaint is built with **WinUI 3** and **Windows App SDK** using pure unpackaged architecture (`<WindowsPackageType>None</WindowsPackageType>`), requiring no MSIX packaging or certificates.

- `MainWindow.xaml` / `MainWindow.xaml.cs`: App navigation shell with mica backdrop and navigation pane.
- `Views/StudioPage.xaml`: Full-featured creative canvas with brush bar, responsive palette, and canvas viewport.
- `Views/StudioPage.Paint.cs`: Core rendering engine using Win2D hardware-accelerated canvas sessions, layered strokes, and procedural physics.
- `Views/StudioPage.IO.cs`: Composite rendering, PNG export, and local persistence.
- `Views/ColoringGalleryPage.xaml`: Procedurally generated vector coloring pages catalog.
- `Views/ArtworkPage.xaml`: Saved local artwork gallery with thumbnail index.
- `Views/EffectsPage.xaml`: Photo import, camera capture filter pipeline (sketch, sepia, blur, monochrome).
- `Services/ColoringArt.cs`: Procedural vector line-art drawing engine.
- `Services/ArtworkStore.cs`: Indexing and storage management under `%LOCALAPPDATA%\PaperPaint\Artwork`.
- `Helpers/`: Utility classes for image operations, windowing, and unpackaged settings.

---

## 💡 How You Can Help

- 🎨 **New Coloring Pages**: Add new vector illustrations to `Services/ColoringArt.cs`.
- 🖌️ **New Brushes & Tools**: Expand canvas brush dynamics (e.g. calligraphy, airbrush, palette knife).
- ⚡ **Performance & Win2D**: Optimize rendering loops, stroke interpolation, or layer compositing.
- ♿ **Accessibility & UI**: Keyboard navigation, high-contrast themes, screen reader labels.
- 🌍 **Localization**: Add translations for UI strings and dialogs.

---

## 📋 Submitting Changes

1. **Fork the repo** and create a feature branch from `main`:
   ```bash
   git checkout -b feature/my-cool-feature
   ```
2. **Make your changes** following standard C# coding conventions and existing code style.
3. **Verify the build and publish**:
   ```powershell
   dotnet build "PaperPaint.csproj" -p:Platform=x64
   dotnet publish "PaperPaint.csproj" -c Release -p:Platform=x64 -o publish_out
   ```
4. **Commit and push** to your fork:
   ```bash
   git commit -m "Add new watercolor wet-bloom effect"
   git push origin feature/my-cool-feature
   ```
5. **Open a Pull Request** describing the motivation and changes made.

---

## 💬 Community & Questions

Feel free to open an issue for bug reports, questions, or feature proposals. Let's make digital art accessible, fun, and open to everyone!
