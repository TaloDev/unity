#!/bin/bash

VERSION=$(cat "Assets/Talo Game Services/Talo/VERSION" | tr -d '\n')
REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)

gh pr create \
  --repo "$REPO" \
  --base main \
  --head develop \
  --title "Release $VERSION" \
  --label "release" \
  --body ""
