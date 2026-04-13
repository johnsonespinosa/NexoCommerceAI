# 🚀 NexoCommerceAI – AI-Powered E-commerce Backend

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
