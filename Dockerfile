# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)

# Especifica la base para la imagen
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID

# Direccion donde vivirá nuestra aplicación en el contenedor
WORKDIR /app

# Puerto del contenedor
EXPOSE 8080
EXPOSE 8081


# Esta fase se usa para compilar el proyecto de servicio
# Especifica la base para la imagen una vez compilada ya puedo usar el runtime que es más ligera
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Direccion donde vivirá nuestra aplicación en el contenedor
WORKDIR /src

# Copia los archivos al directorio
COPY ["apiv2.csproj", "."]

# Direccion donde vivirá nuestra aplicación en el contenedor
RUN dotnet restore "./apiv2.csproj"

# Copia todo lo de la ruta actual a la ruta del contenedor
COPY . .
WORKDIR "/src/."
RUN dotnet build "./apiv2.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./apiv2.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Para que no se ejecute net core en vez de powershell en linux
ENTRYPOINT ["dotnet", "apiv2.dll"]