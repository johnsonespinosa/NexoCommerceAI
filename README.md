# 🚀 NexoCommerceAI – AI-Powered E-commerce Backend

[![CI](https://github.com/johnsonespinosa/NexoCommerceAI/actions/workflows/ci.yml/badge.svg)](https://github.com/johnsonespinosa/NexoCommerceAI/actions/workflows/ci.yml)
[![CD Docker](https://github.com/johnsonespinosa/NexoCommerceAI/actions/workflows/cd-docker.yml/badge.svg)](https://github.com/johnsonespinosa/NexoCommerceAI/actions/workflows/cd-docker.yml)
[![Deploy Azure Web App](https://github.com/johnsonespinosa/NexoCommerceAI/actions/workflows/deploy-azure-webapp.yml/badge.svg)](https://github.com/johnsonespinosa/NexoCommerceAI/actions/workflows/deploy-azure-webapp.yml)

A production-ready e-commerce backend built with **Clean Architecture**, **CQRS**, and **AI-powered recommendations**.

This project simulates a real-world scalable system, including authentication, payments, caching, and intelligent product recommendations.

---

# 🧠 Overview

NexoCommerceAI is a modern backend system designed to reflect real-world SaaS architectures.

It includes:

- Scalable API design
- Distributed caching with Redis
- Payment processing (Stripe-style)
- AI-based recommendation engine
- Dockerized deployment

---

# ⚙️ Tech Stack

### Backend
- ASP.NET Core 8
- Entity Framework Core
- MediatR (CQRS pattern)

### Database & Cache
- PostgreSQL
- Redis

### Architecture
- Clean Architecture
- Repository Pattern
- Ardalis Specification

### DevOps
- Docker & Docker Compose
- CI/CD (GitHub Actions)

### AI Features
- Behavior tracking (events)
- Product recommendations
- Trending algorithm
- Personalized suggestions

---

# 🏗️ Architecture

The project follows Clean Architecture principles:
API → Application → Domain → Infrastructure


Key patterns used:

- CQRS with MediatR
- Repository + Specification
- Distributed caching (Redis)
- Event-driven tracking system
- Webhook-based payment confirmation

---

# 🔐 Features

## Authentication
- JWT-based authentication
- Register / Login / Refresh Token

## Product Management
- Product catalog
- Categories
- Stock handling

## Cart System
- Add / Remove items
- Hybrid persistence (DB + Redis)
- Cached cart retrieval

## Orders & Checkout
- Order creation from cart
- Snapshot-based order items
- Total calculation (subtotal, tax, total)

## Payments
- Stripe-style PaymentIntent flow
- Webhook-based confirmation
- Order status lifecycle (Pending → Paid)

## AI Recommendation System
- “Users also bought”
- Trending products ranking
- Personalized suggestions
- Event tracking (view, cart, purchase)

---

# 🤖 AI Engine

The system includes a behavior-based recommendation engine:

### Event Tracking
- Product views
- Add to cart
- Purchases
- Wishlist actions

### Recommendation Logic
- Collaborative filtering (basic)
- Trending score:


### Personalization
- Category-based affinity
- User behavior analysis

---

# 💳 Payment Flow

1. User performs checkout
2. Order is created (Pending)
3. PaymentIntent is generated
4. Payment is processed
5. Webhook confirms payment
6. Order is updated to Paid

---

# 🐳 Run Locally

### Requirements
- Docker
- Docker Compose

### Run

```bash
docker-compose up --build
```

---

# 🚀 Release Flow (Tags)

This repository uses Git tags to trigger Docker image releases to GHCR.

### Release a version

```bash
git checkout main
git pull
git tag v1.0.0
git push origin v1.0.0
```

When pushing a tag like `v1.0.0`, the workflow `cd-docker.yml` publishes a versioned image.

### Docker images

- Registry: `ghcr.io/johnsonespinosa/NexoCommerceAI`
- Tags generated automatically:
  - Branch name
  - Git tag (for example `v1.0.0`)
  - Commit SHA
  - `latest` (default branch only)

---

# ☁️ Deployment (Azure Web App for Containers)

A deployment workflow is included in `.github/workflows/deploy-azure-webapp.yml`.

### Required GitHub secrets

- `AZURE_WEBAPP_NAME` -> your Azure Web App name
- `AZURE_WEBAPP_PUBLISH_PROFILE` -> publish profile XML from Azure

### Deploy options

- Manual: run the workflow from the Actions tab (`workflow_dispatch`)
- Automatic: push a tag `v*` after the Docker image is built
