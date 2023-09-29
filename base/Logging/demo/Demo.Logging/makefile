console-info:
	dotnet run console

console-debug:
	dotnet run console -p:DefineConstants=\"LOG_DEBUG\"

console-trace:
	dotnet run console -p:DefineConstants=\"LOG_DEBUG\;LOG_TRACE\"

start-seq:
	docker run \
		--rm \
		--name seq \
		-d \
		-e ACCEPT_EULA=Y \
		-p 5341:80 \
		datalust/seq:latest

stop-seq:
	docker stop seq

.PHONY: $(MAKECMDGOALS)