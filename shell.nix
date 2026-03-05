{pkgs ? import <nixpkgs> {}}:
pkgs.mkShell {
  nativeBuildInputs = with pkgs; [
    gcc
    dotnet-sdk
    omnisharp-roslyn
    csharpier
    mono
  ];
}
