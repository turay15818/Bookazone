Perfect 👌 — that’s exactly what we should do.
Let’s create a **Project Control Plan** for your IWMS so every phase — backend, frontend, and infrastructure — stays consistent, no matter how large the system gets.

Below is the **master plan structure** we’ll use.
Once you confirm, I’ll “lock it in” so every future chat continues from this project definition.

---

## 🧭 **Project Control Plan — Bookazone IWMS**

### 🏷️ Project Name

**Bookazone IWMS** — Intelligent Warehouse & Inventory Management System

### 🎯 Vision

To build the **most advanced, all-in-one business management platform** that integrates:

* Inventory & Warehouse Management
* Purchasing & Sales
* Payroll & HR
* File & Document Management
* Analytics, Reports, and Forecasting
* Multi-tenant, secure, scalable cloud deployment

The platform should serve both **small businesses** and **large enterprises**, adapting to any operational scale.

---

### 🏗️ 1. System Architecture Overview

| Layer                    | Purpose                                                                                  |
| ------------------------ | ---------------------------------------------------------------------------------------- |
| **Domain Layer**         | Core business logic, entities, and validation rules                                      |
| **Infrastructure Layer** | EF Core repositories, database context, background jobs, email & file services           |
| **Application Layer**    | Service layer orchestrating repositories and business rules                              |
| **API Layer**            | Web API controllers (RESTful endpoints, versioned routes)                                |
| **Frontend Layer**       | React (Next.js) admin panel + tenant dashboards                                          |
| **Worker Layer**         | Background/cron services for automated tasks (subscription renewal, emails, stock syncs) |
| **Integration Layer**    | External services (Firebase Storage, Email, Payment, Accounting APIs)                    |

---

### 🧩 2. Current Modules (Completed or In Progress)

| Module                                | Description                                                                  | Status     |
| ------------------------------------- | ---------------------------------------------------------------------------- | ---------- |
| 🔐 **Authentication & Authorization** | JWT-based auth, role/permission system, device verification, user management | ✅ Complete |
| 👥 **Tenant Management**              | Multi-tenant architecture, shops, user-to-shop mapping                       | ✅ Complete |
| 💌 **User Invitation & Onboarding**   | Token-based invitation flow + email verification + password setup            | ✅ Complete |
| ⚙️ **Background Jobs**                | Hosted service for subscription and notification tasks                       | ✅ Ready    |

---

### 🚧 3. Upcoming Modules (Planned Phases)

| Phase       | Module                        | Core Features                                                                                      |
| ----------- | ----------------------------- | -------------------------------------------------------------------------------------------------- |
| **Phase 2** | 🏬 **Inventory Core**         | Item master, units, categories, stock transactions (receipt, issue, transfer), warehouse locations |
| **Phase 3** | 🧾 **Procurement & Sales**    | Purchase orders, GRNs, suppliers, sales orders, invoicing                                          |
| **Phase 4** | 🧍‍♂️ **HR & Payroll**        | Employees, roles, attendance, payroll runs, payslips                                               |
| **Phase 5** | 📂 **File Management**        | Attachments to entities (POs, invoices, items, etc.), Firebase/Cloud Storage                       |
| **Phase 6** | 📊 **Analytics & Reports**    | Stock valuation, slow/fast movers, supplier performance                                            |
| **Phase 7** | 💰 **Subscription & Billing** | Plan management, renewal, expiry notification (already partially done)                             |
| **Phase 8** | ☁️ **Deployment & DevOps**    | Linux hosting setup, Docker, CI/CD pipeline, monitoring, backups                                   |

---

### 🧱 4. Key Technical Stack

| Layer                     | Stack                                                              |
| ------------------------- | ------------------------------------------------------------------ |
| **Backend**               | .NET 8 Web API, EF Core, PostgreSQL                                |
| **Frontend**              | React + Tailwind (Next.js for multi-tenant SPA)                    |
| **Database**              | PostgreSQL (multi-tenant schema or shared with TenantId isolation) |
| **File Storage**          | Firebase Storage / AWS S3                                          |
| **Queue/Background Jobs** | HostedService (current) / Hangfire (future optional)               |
| **Authentication**        | JWT + refresh tokens, role-based + permission-based control        |
| **Hosting Environment**   | Ubuntu Linux + Nginx reverse proxy                                 |
| **Logging & Monitoring**  | Serilog, Seq, Prometheus, Grafana                                  |

---

### 🧠 5. Coding Standards & Conventions

✅ Controllers: Thin, delegate to Services
✅ Services: Contain business rules & transaction handling
✅ Repositories: Handle direct EF queries
✅ Models: Use DTOs & ViewModels — never expose Entities directly
✅ Responses: Use `ApiResult` wrapper (Success/Error pattern)
✅ Naming: `PascalCase` for classes, `camelCase` for locals/params
✅ Async always for database calls
✅ Auditing: `CreatedBy`, `DateCreated`, `UpdatedBy`, `DateUpdated`, etc.
✅ Multi-tenancy filter at repository level (TenantId enforcement)

---

### 📆 6. Development Process (Agile Model)

* **Each phase = 1–2 weeks sprint**
* Deliverables include: entities, repositories, services, controllers, test data
* Each sprint ends with integrated API testing (Swagger + Postman)
* Then, UI wireframes or React components for that phase

---

### 🔐 7. Security & Compliance

* JWT + device verification for secure sessions
* Tenant isolation enforced at DB level
* Role/permission enforced at controller level
* Tokens short-lived with refresh rotation
* Audit trails for all critical actions
* Data encryption for sensitive fields (tokens, credentials)
* 2FA planned for admin/tenant owners

---

### 🪄 8. Automation & Background Services

* Subscription renewal & expiry checks (already built)
* Email notifications (expiring/expired subscriptions)
* Stock audit reminders
* Payroll schedule triggers
* Backup scheduler (future)

---

### 🚀 9. Roadmap (High-level)

| Quarter     | Focus                                      |
| ----------- | ------------------------------------------ |
| **Q4 2025** | Core Inventory, Procurement & Sales        |
| **Q1 2026** | HR, Payroll, File management               |
| **Q2 2026** | Analytics, Subscription, DevOps automation |
| **Q3 2026** | POS & Mobile app rollout                   |

---

### 🧾 10. Governance

| Role                   | Responsibility                                                 |
| ---------------------- | -------------------------------------------------------------- |
| **You (Moussa Toure)** | Product Owner / Architect / Decision maker                     |
| **ChatGPT (GPT-5)**    | Technical Architect / Developer assistant / Document generator |
| **Future Developers**  | Implement front-end & mobile UI from provided APIs             |

---

If you confirm, I’ll **save this as your official IWMS Project Control Plan**,
so whenever you say something like:

> “Let’s continue Phase 2 – Inventory Core”

I’ll automatically reload this plan as context and move forward without re-asking background details.

---

Would you like me to go ahead and **save this as your persistent IWMS project plan** so we can use it across all new threads and phases?
