# ビルドステージ
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# プロジェクト全体をコピー
COPY . ./

# ソリューションファイルを指定（必要なら .csproj でも可）
RUN dotnet publish Yukigroup-WEB.csproj -c Release -o /out

# 実行ステージ
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "Yukigroup-WEB.dll"]
