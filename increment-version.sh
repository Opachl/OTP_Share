#!/bin/bash

# Read the current version from version.txt
VERSION=$(cat version.txt)

# Split the version into major, minor, and patch
IFS='.' read -r MAJOR MINOR PATCH <<< "$VERSION"

# Increment the patch version
PATCH=$((PATCH + 1))

# Combine the new version
NEW_VERSION="$MAJOR.$MINOR.$PATCH"

# Write the new version back to version.txt
echo "$NEW_VERSION" > version.txt

# Output the new version
echo "$NEW_VERSION"
