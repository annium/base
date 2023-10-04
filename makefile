format:
	xs format -sc -ic

setup:
	xs remote restore -user $(user) -password $(pass)

update:
	xs update all -sc -ic

clean:
	xs clean -sc -ic

build:
	dotnet build --nologo -v q

test:
	dotnet test --nologo -v q

publish:
	xs publish all 0.1.0 -p 1

keys:
	openssl req -x509 -nodes -days 3650 -keyout key.pem -out cert.pem
	openssl pkcs12 -export -inkey key.pem -in cert.pem -out cert.pfx

.PHONY: $(MAKECMDGOALS)
