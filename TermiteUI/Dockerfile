# Базовый образ для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем весь проект и восстанавливаем зависимости
COPY . . 
RUN dotnet restore "TermiteUI.csproj"

# Собираем проект
RUN dotnet publish "TermiteUI.csproj" -c Release -o /app/publish

# Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app

# Копируем скомпилированный код
COPY --from=build /app/publish .

# Устанавливаем необходимые параметры для Avalonia UI
# Если необходимо, уберите или оставьте в зависимости от особенностей вашего приложения
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Указываем точку входа для приложения
ENTRYPOINT ["dotnet", "TermiteUI.dll"]
