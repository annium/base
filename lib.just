# Shared recipes for Annium .NET sub-projects.
#
# DO NOT EDIT IN PLACE. This file is owned by the umbrella repo at `ci/lib.just` and copied
# into each sub-project by `just copy-ci` there. Local edits are overwritten on the next copy.
#
# The importing justfile must declare:
#
#     set shell := ["bash", "-cu"]
#     set positional-arguments
#     set allow-duplicate-recipes := true
#
#     import 'lib.just'
#
# `allow-duplicate-recipes` is what makes overriding work: a recipe redefined in the
# importing justfile wins over the one here. Override rather than edit — e.g. `test` differs
# per repo (test runner), as does `update` (whether `xs update all` takes `dotnet`).

[private]
default:
    @just --list

# base

setup:
    @echo "=== $0 ==="
    dotnet tool restore

format:
    @echo "=== $0 ==="
    dotnet tool run csharpier format . --config-path $(pwd)/.editorconfig
    dotnet tool run xs format -sc -ic

format-full: format
    @echo "=== $0 ==="
    dotnet format style
    dotnet format analyzers

ensure-no-changes:
    #!/usr/bin/env bash
    set -e
    echo "=== ensure-no-changes ==="
    if [[ -n "$(git status --porcelain)" ]]; then
        echo "Changes detected:"
        git status --short
        git --no-pager diff --no-color HEAD
        exit 1
    fi

update:
    @echo "=== $0 ==="
    dotnet tool list --format json | jq -r '.data[] | "\(.packageId)"' | xargs -I% dotnet tool install %
    dotnet tool run xs update all -sc -ic

clean:
    @echo "=== $0 ==="
    dotnet tool run xs clean -sc -ic
    find . -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -delete

build:
    #!/usr/bin/env bash
    set -e
    echo "=== build ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet build -c Release --nologo -v q -p:PackageVersion=$packageVersion

test:
    @echo "=== $0 ==="
    dotnet test -c Release --no-build --report-xunit-trx

pack:
    #!/usr/bin/env bash
    set -e
    echo "=== pack ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg -p:PackageVersion=$packageVersion

publish apiKey:
    @echo "=== $0 ==="
    dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key "$1" --skip-duplicate
    find . -type f \( -name '*.nupkg' -o -name '*.snupkg' \) -delete

# docs

docs-lint:
    @echo "=== $0 ==="
    dotnet tool run doclint lint -w . -i '**/*.cs' -e '**/obj/**/*.cs'

docs-clean:
    @echo "=== $0 ==="
    rm -rf _site api

docs-metadata:
    @echo "=== $0 ==="
    dotnet tool run docfx metadata docfx.json

docs-build:
    @echo "=== $0 ==="
    dotnet tool run docfx docfx.json

docs-serve:
    @echo "=== $0 ==="
    dotnet tool run docfx serve _site

docs-watch:
    @echo "=== $0 ==="
    dotnet tool run docfx docfx.json --serve

# ci

ci-merge-request-short:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-merge-request-short ==="
    just setup
    just format
    just ensure-no-changes
    just clean
    just build
    just docs-lint

ci-merge-request-full:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-merge-request-full ==="
    just setup
    just format
    just ensure-no-changes
    just clean
    just build
    just docs-lint
    just test

ci-release apiKey repository githubToken:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-release ==="
    just setup
    just format
    just ensure-no-changes
    just ci-set-package-version
    just clean
    just build
    just docs-lint
    just test
    just pack
    just publish "$1"
    just ci-push-tag "$2" "$3"
    echo "Release complete"

ci-set-package-version:
    @echo "=== $0 ==="
    git config user.name "it"
    git config user.email "it@annium.com"
    dotnet tool run versioning set-version -v $(cat version)

ci-push-tag repository githubToken:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-push-tag ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    git remote set-url origin https://x-access-token:"$2"@github.com/"$1".git
    if git ls-remote --exit-code --tags origin "v$packageVersion" >/dev/null 2>&1; then
        echo "tag v$packageVersion already published, skipping"
        exit 0
    fi
    git push origin v$packageVersion
