#!/bin/bash
set -euo pipefail

VERSION_FILE="Assets/Talo Game Services/Talo/VERSION"
BUMP=${1:-}

bump() {
  local version=$1 part=$2
  IFS=. read -r major minor patch <<< "$version"

  case $part in
    major) major=$((major + 1)); minor=0; patch=0 ;;
    minor) minor=$((minor + 1)); patch=0 ;;
    patch) patch=$((patch + 1)) ;;
    *) return 1 ;;
  esac

  echo "$major.$minor.$patch"
}

if [ -n "$BUMP" ]; then
  VERSION=$(bump "$(tr -d '\n' < "$VERSION_FILE")" "$BUMP") || {
    echo "Usage: $0 [major|minor|patch]"
    exit 1
  }

  echo "$VERSION" > "$VERSION_FILE"
  git add "$VERSION_FILE"
  git commit -m "Bump version to $VERSION"
fi

VERSION=$(tr -d '\n' < "$VERSION_FILE")
REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)

git push

gh pr create \
  --repo "$REPO" \
  --base main \
  --head develop \
  --title "Release $VERSION" \
  --label "release" \
  --body ""
