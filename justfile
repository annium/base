set shell := ["bash", "-cu"]
set positional-arguments

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
        git status
        git --no-pager diff --no-color --exit-code
    fi

update:
    @echo "=== $0 ==="
    dotnet tool list --format json | jq -r '.data[] | "\(.packageId)"' | xargs -I% dotnet tool install %
    dotnet tool run xs update all -sc -ic

clean:
    @echo "=== $0 ==="
    dotnet tool run xs clean -sc -ic
    find . -type f -name '*.nupkg' | xargs -I% rm %

build:
    #!/usr/bin/env bash
    set -e
    echo "=== build ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet build -c Release --nologo -v q -p:PackageVersion=$packageVersion

test:
    @echo "=== $0 ==="
    dotnet test -c Release --no-build --nologo --logger "trx;LogFilePrefix=test-results.trx"

pack:
    #!/usr/bin/env bash
    set -e
    echo "=== pack ==="
    packageVersion=$(dotnet tool run versioning get-version -v $(cat version))
    dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg -p:PackageVersion=$packageVersion

publish apiKey:
    @echo "=== $0 ==="
    dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key "$1"
    find . -type f -name '*.nupkg' | xargs -I% rm %

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

# keys

gen-rsa-keys:
    @echo "=== $0 ==="
    openssl req -x509 -noenc -days 3650 -keyout private.pem -out cert.pem
    openssl rsa -in private.pem -pubout -out public.pem
    openssl pkcs12 -export -inkey private.pem -in cert.pem -out cert.pfx
    rm cert.pem

copy-rsa-keys:
    @echo "=== $0 ==="
    cp private.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/rsa_private.pem
    cp public.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/rsa_public.pem
    cp private.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/rsa_private.pem
    cp public.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/rsa_public.pem
    rm private.pem public.pem
    mv cert.pfx base/Net/tests/Annium.Net.Sockets.Tests/keys/rsa_cert.pfx

gen-ec-keys:
    @echo "=== $0 ==="
    openssl req -new -newkey ec -pkeyopt ec_paramgen_curve:secp521r1 -x509 -noenc -days 3650 -keyout private.pem -out cert.pem
    openssl ec -in private.pem -pubout -out public.pem
    openssl pkcs12 -export -inkey private.pem -in cert.pem -out cert.pfx
    rm cert.pem

copy-ec-keys:
    @echo "=== $0 ==="
    cp private.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/ecdsa_private.pem
    cp public.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/ecdsa_public.pem
    cp private.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/ecdsa_private.pem
    cp public.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/ecdsa_public.pem
    rm private.pem public.pem
    mv cert.pfx base/Net/tests/Annium.Net.Sockets.Tests/keys/ecdsa_cert.pfx

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

ci-merge-request-full:
    #!/usr/bin/env bash
    set -e
    echo "=== ci-merge-request-full ==="
    just setup
    just format
    just ensure-no-changes
    just clean
    just build
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
    git push origin v$packageVersion
