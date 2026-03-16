# Azure Lifecycle Sentinel — Product Plan

> **Tagline:** _"Never be blindsided by an Azure change again."_

## 1. Problem Statement

Enterprise organizations (Rabobank, ABN-AMRO, ING, and similar) operate a **platform team ↔ workload team** model where:

- A **Platform Engineering team** owns the Azure landing zone, Entra ID, subscriptions, policies, and guardrails.
- **Workload teams** receive vended subscriptions and build on top of the platform under policy enforcement.

**The gap:** When Azure announces lifecycle changes — certificate deprecations, SKU retirements, API version sunsets, TLS policy changes, regional capacity shifts — **neither team has a systematic way to**:

1. **Discover** the change before it becomes urgent
2. **Map** the change to their actual deployed resources across 50–500+ subscriptions
3. **Route** the notification to the right team (platform vs. workload) based on who owns the affected resource
4. **Track** that the remediation was completed before the deadline
5. **Report** their lifecycle readiness posture to leadership

The result: reactive firefighting, missed deadlines, production incidents, audit findings, and friction between platform and workload teams.

---

## 2. Product Vision

**Azure Lifecycle Sentinel** is an AI-powered lifecycle management platform that continuously monitors Azure's change landscape, maps changes to an organization's deployed resources, and orchestrates the response across platform and workload teams — turning lifecycle management from a reactive scramble into a proactive, measured, and auditable process.

---

## 3. Core Capabilities & Feature Deep-Dive

### 3.1 🛰️ Azure Update Radar — Multi-Source Change Ingestion

The foundation of the system: a daemon that runs on a schedule (e.g., every 6 hours) and ingests change signals from multiple Azure sources.

**Data Sources:**

| Source | What It Catches | Ingestion Method |
|--------|----------------|------------------|
| [Azure Updates RSS](https://azure.microsoft.com/en-us/updates/?query=) | New features, deprecations, retirements, previews | RSS/Atom feed polling |
| Azure Service Health (Planned Maintenance) | Maintenance windows, health advisories, security advisories | Azure Resource Graph / REST API |
| Azure Advisor Recommendations | Deprecation recommendations, cost/security/reliability suggestions | Advisor REST API |
| Azure Resource Graph Change Tracking | Actual resource configuration drift | ARG `resourcechanges` table |
| Microsoft Security Response Center (MSRC) | Critical security bulletins affecting Azure services | MSRC API / RSS |
| Azure SDK & API Version Lifecycle | API version deprecations, SDK end-of-support | GitHub release feeds + docs scraping |

**AI-Powered Parsing (per ingested entry):**

The daemon uses Azure OpenAI (GPT-4o) to analyze each raw update entry and extract structured metadata:

```json
{
  "title": "Azure Cache for Redis: TLS 1.0 and 1.1 retirement",
  "source": "azure-updates-rss",
  "changeType": "retirement",          // retirement | deprecation | breaking-change | security-advisory | new-feature | migration-required
  "severity": "critical",               // critical | high | medium | low | informational
  "affectedServices": ["Microsoft.Cache/redis"],
  "affectedResourceTypes": ["Microsoft.Cache/redis"],
  "affectedSkus": ["Basic", "Standard", "Premium"],
  "affectedRegions": ["*"],             // or specific regions
  "affectedApiVersions": [],
  "actionRequired": "Migrate all Redis instances to TLS 1.2+ before 2026-09-30",
  "deadline": "2026-09-30",
  "effortEstimate": "medium",           // low | medium | high | very-high
  "migrationPath": "Update minTlsVersion property to '1.2' on all Redis instances",
  "microsoftDocLinks": ["https://learn.microsoft.com/..."],
  "aiConfidence": 0.94,                 // How confident the AI is in its parsing
  "rawContent": "..."
}
```

**Why this matters to platform teams:** Instead of manually reading Azure update emails and blog posts, the system does the reading *for them* — and translates marketing-speak into precise, actionable, resource-type-mapped intelligence.

---

### 3.2 🗺️ Cross-Subscription Resource Cartographer

The system maintains a live resource inventory across all subscriptions using **Azure Resource Graph** with a service principal that has `Reader` role at the management group level.

**Capabilities:**

- **Full Resource Inventory:** Continuously scans and caches all resources across all subscriptions using Azure Resource Graph queries.
- **Tag-Based Ownership Resolution:** Uses a configurable tag taxonomy to determine:
  - Which **workload team** owns each resource (`team`, `costCenter`, `applicationId`)
  - Which **platform component** a resource belongs to (`platform-component: networking`, `platform-component: identity`)
  - Contact information (`teamEmail`, `teamChannel`, `serviceNowGroup`)
- **Subscription-to-Team Mapping:** Maintains a mapping of subscription → owning team (sourced from tags, CMDB, or manual config).
- **Resource Dependency Graph:** Understands which resources depend on each other (e.g., an App Service that uses a Redis Cache that uses a VNet).

**Sample Resource Graph Queries the system runs:**

```kusto
// Find all Redis instances across all subscriptions with their TLS version
resources
| where type == "microsoft.cache/redis"
| project subscriptionId, resourceGroup, name, 
          tlsVersion = properties.minimumTlsVersion,
          sku = sku.name,
          tags
| extend team = tostring(tags.team),
         teamEmail = tostring(tags.teamEmail)
```

**Why this matters:** Platform teams today have *no easy way* to answer: "How many of our 200 workload teams are affected by this Redis TLS change?" This feature answers that in seconds.

---

### 3.3 💥 Impact Blast Radius Analyzer

When the Radar detects a change and the Cartographer knows what's deployed, the **Blast Radius Analyzer** connects the dots.

**For every detected change, the system computes:**

1. **Affected Resource Count** — How many resources match the affected resource type/SKU/configuration?
2. **Affected Subscriptions** — Which subscriptions contain affected resources?
3. **Affected Teams** — Which workload teams own affected resources? Is the platform team also affected?
4. **Dependency Cascade** — If a VNet is affected, what App Services, AKS clusters, and databases sit on that VNet?
5. **Geographic Distribution** — Are affected resources concentrated in one region or spread globally?
6. **Severity Amplification** — AI adjusts severity based on:
   - Number of affected resources (10 Redis instances = higher than 1)
   - Whether production subscriptions are affected (prod > dev)
   - Time until deadline (< 30 days = escalate)
   - Whether the affected resource is a shared platform resource vs. a workload resource

**Output: Impact Report**

```
╔══════════════════════════════════════════════════════════════╗
║  IMPACT REPORT: Azure Cache for Redis TLS 1.0/1.1 Retirement
║  
║  Severity: 🔴 CRITICAL (amplified from HIGH)
║  Deadline: 2026-09-30 (198 days remaining)
║  
║  BLAST RADIUS:
║  ├── 47 Redis instances affected across 23 subscriptions
║  ├── 12 workload teams impacted
║  ├── 3 platform-shared Redis instances (networking, identity, monitoring)
║  ├── 156 downstream resources depend on affected Redis instances
║  └── Regions: West Europe (38), North Europe (9)
║  
║  TEAMS AFFECTED:
║  ├── Platform Team (3 shared instances) — CRITICAL
║  ├── Team Payments (8 instances, 2 prod) — HIGH
║  ├── Team Lending (5 instances, 1 prod) — HIGH
║  ├── Team Mobile (4 instances, 0 prod) — MEDIUM
║  └── ... 9 more teams
║  
║  AI-GENERATED ACTION:
║  "Update the minimumTlsVersion property to '1.2' on all 47
║   Redis instances. This is a non-breaking change for clients
║   already using TLS 1.2+. Verify client TLS versions first
║   using Azure Monitor connection logs."
╚══════════════════════════════════════════════════════════════╝
```

---

### 3.4 📡 Smart Notification Router

Not every team needs every notification. The router ensures the right people see the right things at the right time.

**Routing Logic:**

| Condition | Route To | Channel |
|-----------|----------|---------|
| Platform-shared resource affected | Platform Team | Teams channel + PagerDuty (if critical) |
| Workload resource affected | Owning workload team | Teams channel + email |
| Policy change required | Platform Team (policy owners) | Teams + Azure DevOps work item |
| Security advisory | Platform Security Team + affected workload teams | Urgent: Teams + email + phone |
| Deadline < 30 days, no action taken | Escalation: Team lead → Engineering Manager → CTO | Escalation chain |

**Notification Modes:**

- **🔴 Real-time Alert** — For critical/security items. Immediate push to Teams, email, PagerDuty.
- **🟡 Daily Digest** — Morning summary of new changes detected in the last 24 hours, grouped by team.
- **🔵 Weekly Lifecycle Report** — Comprehensive weekly report showing all active lifecycle items, their status, and countdown timers.
- **⚪ Monthly Executive Brief** — High-level posture report for leadership.

**Integration Targets:**

- Microsoft Teams (Adaptive Cards with action buttons)
- Email (HTML formatted reports)
- Azure DevOps (Auto-create work items)
- ServiceNow (Auto-create incidents/change requests)
- Slack (webhook integration)
- PagerDuty / OpsGenie (for critical alerts)
- Power Automate (custom workflow triggers)

**Adaptive Card Example (Teams):**

The Teams notification includes action buttons:
- ✅ "Acknowledge" — Team confirms they've seen it
- 📋 "Create Work Item" — Auto-creates an Azure DevOps work item with pre-filled details
- 🔇 "Snooze 7 days" — Delays next reminder (with audit trail)
- 📊 "View Impact Report" — Opens the full blast radius report in the web dashboard

---

### 3.5 📅 Retirement Countdown Dashboard

A visual command center showing all active lifecycle events and their status.

**Dashboard Panels:**

1. **Timeline View** — Horizontal timeline showing all upcoming deadlines with color-coded urgency:
   - 🟢 Green: > 90 days, action planned
   - 🟡 Amber: 30-90 days, action in progress
   - 🔴 Red: < 30 days, action incomplete
   - ⬛ Black: Deadline passed, still not remediated

2. **Heatmap View** — Grid of subscriptions × lifecycle events showing affected/not-affected/remediated status per cell.

3. **Team Scorecard** — Per-team view showing:
   - Active lifecycle items assigned to them
   - Remediation progress (% resources migrated)
   - Average response time (days from notification to first action)
   - Overdue items count

4. **Resource Drill-Down** — Click any lifecycle event to see every affected resource, its current state, and whether it's been remediated.

5. **Trend Charts:**
   - Lifecycle events over time (is Azure accelerating deprecations?)
   - Team response times trending
   - Open vs. closed lifecycle items

---

### 3.6 🤖 AI Action Plan Generator

For each lifecycle event, the AI generates a detailed, team-specific remediation plan.

**What the AI generates:**

```markdown
## Remediation Plan: Redis TLS 1.2 Migration
### For: Team Payments (8 instances)

**Risk Level:** HIGH — 2 production instances affected
**Estimated Effort:** 4 hours (2 hours testing, 2 hours deployment)
**Recommended Window:** Next sprint (before 2026-06-15)

### Pre-Migration Checklist:
1. [ ] Audit client applications connecting to each Redis instance
2. [ ] Verify all clients support TLS 1.2 (check connection strings)
3. [ ] Review Azure Monitor logs for TLS 1.0/1.1 connections (KQL query below)
4. [ ] Test TLS 1.2 enforcement in dev/staging environment first

### KQL Query to Check Current TLS Versions:
AzureDiagnostics
| where ResourceType == "REDIS"
| where Category == "ConnectedClientList"
| summarize by ClientIp, TlsVersion
| where TlsVersion in ("1.0", "1.1")

### Migration Steps:
1. Update Bicep/Terraform: Set `minimumTlsVersion: '1.2'`
2. Deploy to dev → run integration tests
3. Deploy to staging → run smoke tests
4. Deploy to prod during maintenance window
5. Verify no TLS 1.0/1.1 connections in logs for 48 hours

### Rollback Plan:
Set `minimumTlsVersion` back to `1.0` if client connectivity issues arise.
```

**AI also generates:**
- Bicep/Terraform code snippets for the fix
- Azure CLI commands for manual remediation
- Estimated cost impact (if SKU migration is involved)
- Links to relevant Microsoft Learn documentation

---

### 3.7 📊 Compliance & Readiness Scorecard

Enterprise leadership needs to answer: **"How ready are we for upcoming Azure changes?"**

**Scoring Model:**

Each team gets a **Lifecycle Readiness Score (0-100)** based on:

| Factor | Weight | Description |
|--------|--------|-------------|
| Coverage | 25% | % of lifecycle events acknowledged |
| Timeliness | 25% | Average days from notification to first action |
| Completion | 30% | % of affected resources remediated before deadline |
| Overdue Items | 20% | Penalty for items past their deadline |

**Dashboard Views:**

- **Organization-wide Score** — "Rabobank Azure Lifecycle Readiness: 78/100"
- **Per-Team Breakdown** — Ranked table of all teams with their scores
- **Trend Over Time** — Is the organization getting better or worse at lifecycle management?
- **Risk Register** — Sorted list of the highest-risk unresolved lifecycle items
- **Audit Trail** — Complete history of who was notified, when they acknowledged, what action they took

**Executive Export:**
- PDF/PowerPoint report suitable for board meetings
- Quarterly lifecycle readiness trends
- Benchmarking against industry peers (anonymized, future feature)

---

### 3.8 🔍 Proactive Drift Detection

Don't wait for Azure to announce a retirement — **detect that your resources are already using deprecated configurations.**

**What Drift Detection Catches:**

- Resources using **old API versions** (e.g., deploying with `2021-01-01` when `2024-06-01` is current)
- Resources on **deprecated SKUs** (e.g., Basic tier App Service Plans)
- Resources using **sunset TLS versions**
- Resources with **deprecated OS images** (e.g., Windows Server 2016 on VMs)
- Resources in **regions slated for reduced investment**
- Resources using **classic** (ASM) deployment model remnants
- Storage accounts not yet migrated to **minimum TLS 1.2**
- Key Vault certificates using **soon-to-expire CA roots**

**How it works:**

1. Azure Resource Graph queries identify current resource configurations
2. A maintained **deprecation knowledge base** (seeded from Azure updates, enriched by AI) defines what's "outdated"
3. Delta comparison flags resources that are drifting toward risk
4. Results feed into the same notification and tracking pipeline

**Why this thrills platform teams:** They can *proactively* clean up technical debt before Azure forces their hand. It's like a continuous compliance scan for lifecycle currency.

---

### 3.9 🔮 "What-If" Scenario Analyzer

Platform architects need to plan ahead. The What-If Analyzer lets them ask hypothetical questions:

- **"What if Microsoft retires the Basic SKU for Azure SQL?"** → Shows all affected databases, owning teams, estimated migration effort, and cost delta.
- **"What if West Europe region has a capacity constraint?"** → Shows all resources in that region, which could be moved, and the networking implications.
- **"What if TLS 1.2 becomes the minimum everywhere?"** → Scans all services for TLS configuration and shows the gap.
- **"What if Kubernetes 1.27 is deprecated?"** → Finds all AKS clusters on that version and generates upgrade plans.

**How it works:**

1. User describes a hypothetical scenario in natural language
2. AI translates it into resource type + property + condition filters
3. Azure Resource Graph is queried with those filters
4. Blast Radius Analyzer computes the impact
5. Action Plan Generator creates remediation guidance

This is the **"crystal ball"** feature that makes platform architects look like heroes in planning meetings.

---

### 3.10 🔗 ITSM & DevOps Integration Hub

Enterprise teams don't work in isolation — they use ServiceNow, Azure DevOps, Jira, and other tools. Lifecycle Sentinel integrates deeply.

**Azure DevOps Integration:**
- Auto-create work items (User Stories or Tasks) in the appropriate team's backlog
- Include all context: impact report, remediation steps, affected resources, deadline
- Track work item status back in Sentinel (bi-directional sync)
- Link related work items (e.g., all TLS migration tasks across teams)

**ServiceNow Integration:**
- Create Change Requests with pre-filled risk assessment
- Create Incidents for overdue lifecycle items
- Populate the CMDB with lifecycle metadata
- Trigger approval workflows for production changes

**Jira Integration:**
- Create issues with labels, components, and priority mapped from severity
- Bulk-create issues for multi-team impacts

**Custom Webhooks:**
- For any system not natively supported
- JSON payload with full lifecycle event data

---

### 3.11 🏠 Self-Service Portal for Workload Teams

Workload teams shouldn't depend on the platform team to know if they're affected.

**Portal Features:**

- **"Am I Affected?" Query** — Enter your subscription ID or team name, see all active lifecycle items affecting your resources.
- **My Team Dashboard** — Personalized view of:
  - Active lifecycle items for your team
  - Your team's readiness score
  - Your overdue items
  - Your upcoming deadlines
- **Subscribe to Updates** — Choose which service types you care about (e.g., "Notify me about all Redis, AKS, and SQL changes").
- **Remediation Tracker** — Mark resources as remediated, upload evidence, request platform team assistance.
- **Knowledge Base** — Searchable archive of past lifecycle events and how they were resolved (institutional memory).

---

### 3.12 🧠 Predictive Lifecycle Intelligence (Advanced)

Using historical patterns from Azure Updates, the AI can predict upcoming changes:

- **Deprecation Velocity Tracking** — "Azure has been deprecating one API version per service per quarter. At this rate, API version X will likely be deprecated by Q3 2027."
- **Pattern Recognition** — "When Azure deprecates a Basic SKU, they typically announce 12 months ahead. The Basic SKU for Service X hasn't been deprecated yet, but based on patterns, it's likely in the next 6 months."
- **Proactive Recommendations** — "Based on deprecation patterns, we recommend upgrading these 15 resources now rather than waiting for the announcement."
- **Risk Forecasting** — "Your organization has 340 resources on configurations that, based on historical patterns, are likely to be deprecated within the next 18 months."

---

## 4. Technical Architecture

### 4.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                    Azure Lifecycle Sentinel                       │
│                                                                   │
│  ┌──────────────┐  ┌──────────────┐  ┌───────────────────────┐  │
│  │ Azure Update  │  │ Service      │  │ Azure Advisor         │  │
│  │ RSS Feed      │  │ Health API   │  │ API                   │  │
│  └──────┬───────┘  └──────┬───────┘  └───────────┬───────────┘  │
│         │                  │                       │              │
│         └──────────┬───────┴───────────────────────┘              │
│                    ▼                                              │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              📡 INGESTION ENGINE                             │ │
│  │         (Azure Function — Timer Trigger)                     │ │
│  │    Runs every 6 hours / on-demand                            │ │
│  └─────────────────────────┬───────────────────────────────────┘ │
│                            ▼                                      │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              🧠 AI ANALYSIS ENGINE                           │ │
│  │         (Azure OpenAI — GPT-4o)                              │ │
│  │    Parse → Classify → Score → Extract Actions                │ │
│  └─────────────────────────┬───────────────────────────────────┘ │
│                            ▼                                      │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              🗺️ RESOURCE CARTOGRAPHER                       │ │
│  │         (Azure Resource Graph Queries)                       │ │
│  │    Map changes → Deployed resources → Owning teams           │ │
│  └─────────────────────────┬───────────────────────────────────┘ │
│                            ▼                                      │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              💥 BLAST RADIUS ENGINE                          │ │
│  │    Impact scoring, dependency analysis, severity amp.        │ │
│  └─────────────┬─────────────────────────────┬─────────────────┘ │
│                ▼                               ▼                  │
│  ┌──────────────────────┐       ┌──────────────────────────────┐ │
│  │ 📡 NOTIFICATION      │       │ 📊 DASHBOARD & PORTAL        │ │
│  │    ROUTER             │       │    (React SPA / Power BI)    │ │
│  │ Teams/Email/ITSM/     │       │    Countdown, Scorecards,    │ │
│  │ PagerDuty/Webhooks    │       │    What-If, Drill-down       │ │
│  └──────────────────────┘       └──────────────────────────────┘ │
│                                                                   │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              💾 DATA STORE                                   │ │
│  │    Cosmos DB (lifecycle events, impact reports, audit trail) │ │
│  │    Azure SQL (team config, subscription mapping, scores)     │ │
│  └─────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 4.2 Technology Stack

| Component | Technology | Justification |
|-----------|-----------|---------------|
| Daemon / Ingestion | Azure Functions (Timer Trigger, .NET 8) | Serverless, cost-effective, enterprise-familiar |
| AI Analysis | Azure OpenAI Service (GPT-4o) | Enterprise-grade, data stays in Azure, Responsible AI |
| Resource Discovery | Azure Resource Graph | Cross-subscription queries, fast, no agent needed |
| Event Bus | Azure Service Bus | Reliable message delivery, dead-letter support |
| Data Store | Cosmos DB (NoSQL) | Flexible schema for diverse lifecycle events |
| Config Store | Azure SQL | Relational data: teams, subscriptions, routing rules |
| Notifications | Logic Apps + Adaptive Cards SDK | Multi-channel, low-code routing rules |
| Dashboard | React SPA on Azure Static Web Apps | Modern, responsive, SSO with Entra ID |
| Authentication | Entra ID (Managed Identity + App Registration) | Zero-trust, enterprise SSO |
| IaC | Bicep | Native Azure, enterprise-preferred at Dutch banks |
| CI/CD | Azure DevOps Pipelines | Enterprise-standard at target customers |
| Monitoring | Application Insights + Azure Monitor | Full observability of the Sentinel itself |
| Secrets | Azure Key Vault | Enterprise secret management |

### 4.3 Security & Identity Model

- **Managed Identity** for the Function App — `Reader` role at Management Group level for Resource Graph access
- **Entra ID App Registration** for the dashboard — SSO with enterprise tenant
- **RBAC within Sentinel:**
  - `Sentinel Admin` — Full access, manage teams, routing, config
  - `Platform Team Member` — View all, manage platform-level lifecycle items
  - `Workload Team Member` — View own team's items, mark as remediated
  - `Executive Viewer` — Read-only scorecard and reports
- **Data Residency** — All data stored in customer's Azure tenant (sovereign cloud compatible)
- **No data leaves the tenant** — AI processing via customer's own Azure OpenAI deployment

---

## 5. Phased Delivery Roadmap

### Phase 1: 🏗️ Foundation (MVP)

**Goal:** Prove the concept — ingest updates, map to resources, notify teams.

| Todo | Description |
|------|-------------|
| Project scaffolding | .NET 8 Azure Functions project, Bicep IaC, CI/CD pipeline |
| RSS feed ingestion | Timer-triggered function to poll Azure Updates RSS feed |
| AI parser | Azure OpenAI integration to parse and classify RSS entries |
| Resource Graph integration | Cross-subscription resource discovery and caching |
| Basic impact mapping | Match parsed updates to deployed resource types |
| Teams notification | Send Adaptive Cards to configured Teams channels |
| Cosmos DB storage | Store lifecycle events, impact reports, notification history |
| Bicep deployment | Full IaC for all Azure resources |
| Basic CLI dashboard | Console output showing active lifecycle items |

### Phase 2: 🧠 Intelligence

**Goal:** Make the analysis deep and actionable.

| Todo | Description |
|------|-------------|
| Service Health integration | Ingest planned maintenance and health advisories |
| Azure Advisor integration | Ingest deprecation and best-practice recommendations |
| Blast Radius Engine | Full impact analysis with dependency chains |
| AI Action Plan Generator | Per-team remediation plans with code snippets |
| Retirement Countdown API | REST API serving countdown data for dashboards |
| Email digest | Daily/weekly HTML email summaries |
| Severity amplification | AI-adjusted severity based on resource count, environment, deadline |

### Phase 3: 📊 Enterprise Dashboard

**Goal:** Give platform teams a visual command center.

| Todo | Description |
|------|-------------|
| React SPA dashboard | Static Web App with Entra ID SSO |
| Timeline view | Visual countdown of all active lifecycle events |
| Heatmap view | Subscription × lifecycle event matrix |
| Team scorecard | Per-team readiness scoring and ranking |
| Drill-down views | Click-through to affected resources and remediation status |
| Executive report export | PDF/PowerPoint generation for leadership |
| Self-service "Am I Affected?" | Query interface for workload teams |

### Phase 4: 🔗 Integration & Advanced

**Goal:** Embed Sentinel into enterprise workflows.

| Todo | Description |
|------|-------------|
| Azure DevOps work item sync | Auto-create and bi-directionally sync work items |
| ServiceNow integration | Change Requests, Incidents, CMDB updates |
| Drift Detection Engine | Proactive scanning for deprecated configurations |
| What-If Analyzer | Natural language scenario planning |
| Predictive Intelligence | ML-based deprecation forecasting |
| Multi-tenant support | SaaS model for serving multiple enterprises |
| API & SDK | Public API for custom integrations |

---

## 6. Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Language | C# / .NET 8 | Enterprise standard at Dutch banks, Azure Functions native support |
| AI Model | Azure OpenAI (not public OpenAI) | Data sovereignty — enterprise data never leaves Azure tenant |
| Database | Cosmos DB + Azure SQL | Cosmos for flexible event data, SQL for relational config |
| Dashboard | React SPA (not Power BI) | More customizable, better UX, embeddable, lower licensing cost |
| IaC | Bicep (not Terraform) | Native Azure, preferred by target customers, simpler for Azure-only |
| Notifications | Logic Apps (not custom code) | Low-code routing rules, easy to modify, enterprise audit trail |
| Resource Discovery | Resource Graph (not per-sub API calls) | 1000x faster, cross-subscription, no agent deployment |
| Identity | Managed Identity + Entra ID | Zero-trust, no stored credentials, enterprise SSO |

---

## 7. What Makes This Thrilling for Enterprise Platform Teams

1. **"Finally, a single pane of glass for lifecycle risk."** — Platform teams today use Excel spreadsheets and tribal knowledge. This replaces that with a live, AI-powered system.

2. **"It tells ME before Azure tells me."** — By combining RSS, Service Health, Advisor, and drift detection, the system catches things before they become urgent.

3. **"I know exactly which team to call."** — Tag-based ownership resolution means no more "who owns this resource?" detective work.

4. **"My CTO stops asking 'are we ready?' — they can see the scorecard."** — Executive reporting turns lifecycle management from an invisible activity into a measurable one.

5. **"Workload teams can self-serve."** — The self-service portal reduces the platform team's ticket volume and empowers workload teams.

6. **"AI does the reading and thinking."** — Instead of a platform engineer spending 2 hours parsing Azure update emails, AI does it in seconds and generates the remediation plan.

7. **"It integrates with our existing tools."** — ServiceNow, Azure DevOps, Teams — it meets teams where they already work.

8. **"We can prove to auditors that we're managing lifecycle risk."** — Full audit trail of notifications, acknowledgments, and remediations.

9. **"What-If lets us plan proactively."** — Instead of reacting to deprecations, platform architects can scenario-plan ahead.

10. **"It pays for itself."** — One avoided production incident from a missed deprecation saves more than the entire cost of running Sentinel.

---

## 8. Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Mean Time to Awareness | < 24 hours from Azure announcement | Time delta: Azure publish date → first Sentinel notification |
| Mean Time to Acknowledge | < 48 hours from notification | Time delta: notification sent → team acknowledges |
| Lifecycle Readiness Score | > 80/100 org-wide | Weighted scoring model |
| Zero Missed Deadlines | 0 resources still on deprecated config after deadline | Post-deadline Resource Graph scan |
| Team Adoption | > 90% of teams actively using Sentinel | Login/acknowledgment metrics |
| Reduction in Lifecycle Incidents | 50% reduction in P1/P2 incidents caused by lifecycle changes | Incident correlation analysis |

---

## 9. Implementation Todos (Phase 1 — MVP)

These are the concrete implementation tasks for the initial build:

1. **project-scaffold** — Initialize .NET 8 Azure Functions project with solution structure, .editorconfig, Directory.Build.props
2. **bicep-infra** — Create Bicep templates for: Function App, Cosmos DB, Azure OpenAI, Key Vault, Service Bus, Managed Identity, RBAC assignments
3. **rss-ingestion** — Timer-triggered function to poll Azure Updates RSS, parse XML, deduplicate, store raw entries
4. **ai-parser** — Azure OpenAI integration to analyze raw RSS entries and extract structured lifecycle metadata
5. **resource-graph-scanner** — Cross-subscription Resource Graph query engine with caching and tag-based ownership resolution
6. **impact-mapper** — Match AI-parsed updates to discovered resources, compute affected subscriptions/teams/resources
7. **blast-radius-calculator** — Compute severity amplification, dependency chains, and generate impact reports
8. **notification-engine** — Service Bus consumer that routes impact reports to Teams via Adaptive Cards
9. **cosmos-data-layer** — Repository pattern for Cosmos DB: lifecycle events, impact reports, notification history, audit trail
10. **config-management** — Azure SQL schema and API for team definitions, subscription mappings, routing rules, tag taxonomy
11. **cicd-pipeline** — Azure DevOps pipeline: build, test, Bicep deploy, Function App deploy
12. **integration-tests** — End-to-end tests with mock RSS feed, mock Resource Graph, verify notification output
