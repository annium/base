set shell := ["bash", "-cu"]
set positional-arguments
# lib.just is copied in by the umbrella repo's `just copy-ci`; recipes redefined below
# override the shared ones.
set allow-duplicate-recipes := true

import 'lib.just'

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
