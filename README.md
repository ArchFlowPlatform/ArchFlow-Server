# 🛠️ Agile Tracker – MVP

## 📌 Visão Geral
Este projeto consiste em um **sistema web** que centraliza conveniências na **gestão de projetos de desenvolvimento de software**, integrando **metodologias ágeis (Scrum + Kanban)** com recursos de **dashboard analítico**, **documentação** e **diagramas colaborativos**.  

O objetivo é fornecer uma plataforma intuitiva, colaborativa e escalável para apoiar equipes no acompanhamento de projetos com base em **DDD (Domain-Driven Design)**.

---

## 📑 Índice
1. [Objetivo](#-objetivo)
2. [Funcionalidades Principais](#-funcionalidades-principais)
3. [Regras de Negócio](#-regras-de-negócio)
4. [Tecnologias e Arquitetura](#-tecnologias-e-arquitetura)
5. [Entregáveis do MVP](#-entregáveis-do-mvp)
6. [Modelo de Banco de Dados](#-modelo-de-banco-de-dados)

---

## 🎯 Objetivo
Desenvolver uma plataforma que permita que equipes organizem e acompanhem projetos de forma visual, colaborativa e orientada a valor, unindo conceitos de **Scrum** (backlog, sprints, papéis) e **Kanban** (quadros, cartões, limites de WIP).

---

## 🚀 Funcionalidades Principais

### 🔹 Gestão de Usuários
- Cadastro e autenticação de usuários.
- Papéis: **owner, scrum master, product owner, developer**.
- Perfis com avatar, permissões e histórico de participação.

### 🔹 Projetos e Times
- Criação de projetos com status (**ativo, arquivado, excluído**).
- Associação de membros com papéis definidos.
- Gestão de múltiplos projetos por usuário.

### 🔹 Backlog e Planejamento
- **Product Backlog**: visão geral do produto, epics e user stories.
- **Epics**: agrupadores de histórias de usuário, com valor de negócio, prioridade e status.
- **User Stories**: contendo persona, critérios de aceitação, complexidade, esforço, dependências e prioridade.

### 🔹 Sprints
- Criação e acompanhamento de sprints, com meta, datas e capacidade.
- Associação de user stories ao sprint (**Sprint Backlog**).
- Controle de progresso (**planned, active, completed**).

### 🔹 Kanban Board
- Quadro de tarefas estilo **Trello/Jira**.
- Colunas customizáveis (com WIP limit e coluna de “done”).
- Cartões representando tarefas ou user stories, com:
  - Assignee
  - Prioridade
  - Prazos
  - Rótulos
  - Anexos
  - Comentários
- Histórico de atividades (moved, updated, assigned etc.).

### 🔹 Dashboard
- Painel com indicadores de desempenho:
  - Progresso por sprint
  - Velocity da equipe
  - Burndown chart
  - Distribuição de tarefas por membro
  - Comparativo planejado vs realizado

### 🔹 Diagramas e Documentação
- Criação e versionamento de **diagramas** (caso de uso, classes, sequência, arquitetura, etc.).
- Páginas de documentação em **Markdown**, hierárquicas e versionadas.
- Possibilidade de vincular documentação a sprints ou user stories.

---

## 📋 Regras de Negócio
1. Cada **projeto** deve possuir um **product backlog único**.  
2. **User stories** só podem existir dentro de **epics**.  
3. Uma **tarefa** pertence a uma **user story**.  
4. O **Kanban** pode ser de nível de projeto ou sprint.  
5. Diagramas e documentação devem manter **versionamento histórico**.  
6. Cada sprint deve ter **datas de início e fim obrigatórias**.  
7. Colunas e cartões no Kanban seguem ordem definida (**position**).  

---

## 🏗️ Tecnologias e Arquitetura
- **Frontend**: [Next.js](https://nextjs.org/) (React, TypeScript, SSR/SSG)  
- **Backend**: [.NET 8 (ASP.NET Core)](https://dotnet.microsoft.com/)  
- **Banco de Dados**:  POSTGRESQL
- **Autenticação**: JWT + Refresh Tokens  
- **Arquitetura**:  
  - Aplicação de **DDD (Domain-Driven Design)**  
  - Camadas: **Domain**, **Application**, **Infrastructure**, **Presentation**  
  - Separação clara de responsabilidades entre entidades, serviços e repositórios  
- **Metodologia de Trabalho**: Scrum aplicado ao desenvolvimento do sistema  

---

## 📦 Entregáveis do MVP
1. Autenticação de usuários e criação de projetos.  
2. Product Backlog com epics e user stories.  
3. Kanban funcional integrado ao backlog.  
4. Criação e gestão de sprints.  
5. Dashboard básico com indicadores de progresso.  
6. CRUD de diagramas e documentação.  
7. Banco de dados implementado conforme modelo inicial (`dbAgile.txt`).  

---

## 🗄️ Modelo de Banco de Dados
O modelo de dados utilizado segue o arquivo [`dbAgile.txt`](./dbAgile.txt), que define:  
- Usuários e membros de projetos  
- Product Backlogs, Epics e User Stories  
- Sprints e itens de sprint  
- Kanban (Boards, Colunas, Cards, Labels, Comentários, Atividades)  
- Diagramas e Documentação versionada  

---
