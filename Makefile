.PHONY: watch migration update

watch:
	cd MVC && dotnet watch

migration:
	cd MVC && dotnet ef migrations add $(name) --project ../DAL

update:
	cd MVC && dotnet ef database update --project ../DAL

build:
	cd MVC && dotnet build
