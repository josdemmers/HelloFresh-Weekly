FROM mcr.microsoft.com/dotnet/sdk:6.0 as builder
COPY /HelloFresh-Weekly /app
RUN dotnet publish -c Release -o /out /app/HelloFresh-Weekly.csproj
COPY entrypoint.sh /out/entrypoint.sh

FROM mcr.microsoft.com/dotnet/runtime:6.0 as base
COPY --from=builder /out /app
RUN chmod +x /app/entrypoint.sh