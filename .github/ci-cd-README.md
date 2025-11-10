CI/CD for this repository (GitHub Actions)

This repository contains a frontend (Vite) in `url-shortener-ui` and a backend (.NET 8) in `UrlShortener/UrlShortener.Api`.

What the workflow does
- On push and pull_request to `main` it runs:
  - Frontend: install, lint, build
  - Backend: restore, build, test
- On push to `main` it also builds Docker images and pushes them to GitHub Container Registry (GHCR).
  - If you set Docker Hub secrets, it will also push images to Docker Hub.

Required secrets (optional vs recommended)
- `GITHUB_TOKEN` (provided automatically by GitHub Actions) — used to push to GHCR.
- `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` (optional) — if you want the workflow to also push images to Docker Hub.

Where images are pushed
- GHCR: `ghcr.io/<your-org-or-username>/url-shortener-ui:<sha>` and `ghcr.io/<your-org-or-username>/url-shortener-api:<sha>`
- Docker Hub (if secrets set): `<DOCKERHUB_USERNAME>/url-shortener-ui:<sha>` and `<DOCKERHUB_USERNAME>/url-shortener-api:<sha>`

How to add secrets
1. Go to your GitHub repository -> Settings -> Secrets and variables -> Actions -> New repository secret.
2. Add `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` (Docker Hub access token) if you want Docker Hub pushes.

Run locally (quick checks)
- Frontend (PowerShell):
  cd url-shortener-ui; npm ci; npm run lint; npm run build
- Backend (PowerShell):
  dotnet restore UrlShortener/UrlShortener.Api/UrlShortener.Api.csproj; dotnet build --configuration Release UrlShortener/UrlShortener.Api/UrlShortener.Api.csproj; dotnet test --no-build UrlShortener/UrlShortener.Api/UrlShortener.Api.csproj
- Build Docker images locally (PowerShell):
  docker build -t url-shortener-ui:local -f url-shortener-ui/Dockerfile ./url-shortener-ui
  docker build -t url-shortener-api:local -f UrlShortener/Dockerfile ./UrlShortener

Notes & next steps
- If you prefer pushing only to Docker Hub (not GHCR), remove or adapt GHCR steps in `.github/workflows/ci-cd.yml`.
- If you want automated releases or deployment to a cloud provider, tell me which provider and I can extend the workflow.
