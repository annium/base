setup:
	dotnet tool restore

format:
	dotnet csharpier format .
	xs format -sc -ic

format-full: format
	dotnet format style
	dotnet format analyzers

update:
	xs update all -sc -ic

clean:
	xs clean -sc -ic
	find . -type f -name '*.nupkg' | xargs rm

buildNumber?=0
build:
	dotnet build -c Release --nologo -v q -p:BuildNumber=$(buildNumber)

test:
	dotnet test -c Release --no-build --nologo -v q

pack:
	dotnet pack --no-build -o . -c Release -p:SymbolPackageFormat=snupkg

publish:
	dotnet nuget push "*.nupkg" --source https://api.nuget.org/v3/index.json --api-key $(shell cat .xs.credentials)
	find . -type f -name '*.nupkg' | xargs rm

gen-rsa-keys:
	openssl req -x509 -noenc -days 3650 -keyout private.pem -out cert.pem
	openssl rsa -in private.pem -pubout -out public.pem 
	openssl pkcs12 -export -inkey private.pem -in cert.pem -out cert.pfx
	rm cert.pem

copy-rsa-keys:
	cp private.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/rsa_private.pem
	cp public.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/rsa_public.pem

	cp private.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/rsa_private.pem
	cp public.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/rsa_public.pem

	rm private.pem public.pem

	mv cert.pfx base/Net/tests/Annium.Net.Sockets.Tests/keys/rsa_cert.pfx

gen-ec-keys:
	openssl req -new -newkey ec -pkeyopt ec_paramgen_curve:secp521r1 -x509 -noenc -days 3650 -keyout private.pem -out cert.pem
	openssl ec -in private.pem -pubout -out public.pem 
	openssl pkcs12 -export -inkey private.pem -in cert.pem -out cert.pfx
	rm cert.pem

copy-ec-keys:
	cp private.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/ecdsa_private.pem
	cp public.pem base/Identity/tests/Annium.Identity.Tokens.Tests/keys/ecdsa_public.pem

	cp private.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/ecdsa_private.pem
	cp public.pem base/Identity/tests/Annium.Identity.Tokens.Jwt.Tests/keys/ecdsa_public.pem

	rm private.pem public.pem

	mv cert.pfx base/Net/tests/Annium.Net.Sockets.Tests/keys/ecdsa_cert.pfx

.PHONY: $(MAKECMDGOALS)
