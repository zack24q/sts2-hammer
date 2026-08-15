#!/usr/bin/env bash

set -euo pipefail

repo_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
manifest="$repo_dir/HammerMod.json"
hammer_output="$repo_dir/.deploy/HammerMod"
ritsulib_output="$repo_dir/.deploy/STS2-RitsuLib"
dist_dir="$repo_dir/dist"

for tool in jq zip shasum; do
    if ! command -v "$tool" >/dev/null 2>&1; then
        echo "Required tool is not installed: $tool" >&2
        exit 1
    fi
done

dotnet_exe="${DOTNET_EXE:-/usr/local/share/dotnet/dotnet}"
if [[ ! -x "$dotnet_exe" ]]; then
    dotnet_exe="$(command -v dotnet || true)"
fi
if [[ -z "$dotnet_exe" || ! -x "$dotnet_exe" ]]; then
    echo "Could not find dotnet. Set DOTNET_EXE to the executable path." >&2
    exit 1
fi

"$dotnet_exe" build "$repo_dir/HammerMod.csproj" -c Release \
    /p:CopyModOnBuild=true \
    /p:RunPckExport=true

mod_version="$(jq -er '.version | select(type == "string" and length > 0)' "$manifest")"
game_version="$(jq -er '.min_game_version | select(type == "string" and length > 0)' "$manifest")"
required_ritsulib_version="$(jq -er '.dependencies[] | select(.id == "STS2-RitsuLib") | .version' "$manifest")"
packaged_ritsulib_version="$(jq -er '.version | select(type == "string" and length > 0)' "$ritsulib_output/mod_manifest.json")"

if [[ "$required_ritsulib_version" != "$packaged_ritsulib_version" ]]; then
    echo "RitsuLib version mismatch: HammerMod requires $required_ritsulib_version, but .deploy contains $packaged_ritsulib_version." >&2
    exit 1
fi

required_files=(
    "$hammer_output/HammerMod.dll"
    "$hammer_output/HammerMod.json"
    "$hammer_output/HammerMod.pck"
    "$ritsulib_output/STS2-RitsuLib.dll"
    "$ritsulib_output/mod_manifest.json"
    "$repo_dir/packaging/STS2_RITSULIB_LICENSE.txt"
)
for required_file in "${required_files[@]}"; do
    if [[ ! -f "$required_file" ]]; then
        echo "Missing package input: $required_file" >&2
        exit 1
    fi
done

timestamp="$(date +%Y%m%d-%H%M%S)"
package_name="HammerMod-${mod_version}-sts2-${game_version}-test-${timestamp}"
stage_dir="$(mktemp -d "${TMPDIR:-/tmp}/hammer-mod-package.XXXXXX")"
mods_dir="$stage_dir/mods"
archive_path="$dist_dir/$package_name.zip"

cleanup() {
    case "$stage_dir" in
        "${TMPDIR:-/tmp}"/hammer-mod-package.*) rm -rf -- "$stage_dir" ;;
        *) echo "Refusing to remove unexpected temporary directory: $stage_dir" >&2 ;;
    esac
}
trap cleanup EXIT

mkdir -p "$mods_dir/HammerMod" "$mods_dir/STS2-RitsuLib" "$dist_dir"

cp "$hammer_output/HammerMod.dll" "$mods_dir/HammerMod/"
cp "$hammer_output/HammerMod.json" "$mods_dir/HammerMod/"
cp "$hammer_output/HammerMod.pck" "$mods_dir/HammerMod/"
cp "$ritsulib_output/STS2-RitsuLib.dll" "$mods_dir/STS2-RitsuLib/"
cp "$ritsulib_output/mod_manifest.json" "$mods_dir/STS2-RitsuLib/"
cp "$repo_dir/packaging/STS2_RITSULIB_LICENSE.txt" "$mods_dir/STS2-RitsuLib/LICENSE.txt"

(
    cd "$stage_dir"
    zip -qry "$archive_path" mods
)
shasum -a 256 "$archive_path" > "$archive_path.sha256"

echo "Created test package:"
echo "  $archive_path"
echo "SHA-256:"
echo "  $archive_path.sha256"
