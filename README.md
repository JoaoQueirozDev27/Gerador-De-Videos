# Gerador de Vídeos

Um projeto em C# para geração automática de vídeos.

## ⚠️ Importante

**Use a branch `versao-nova` para a versão mais recente e estável do projeto.**

```bash
git clone https://github.com/JoaoQueirozDev27/Gerador-De-Videos.git
cd Gerador-De-Videos
git checkout versao-nova
```

A branch `master` contém código legado e não deve ser utilizada para novo desenvolvimento.

## 📋 Sobre

Este projeto é um gerador de vídeos desenvolvido em **C#**, com funcionalidades para automatizar a criação e processamento de conteúdo em formato de vídeo. Conta com múltiplas interfaces de apresentação para diferentes casos de uso.

## 🚀 Como Começar

### Pré-requisitos

- .NET Framework ou .NET Core (compatível com o projeto)
- Visual Studio ou Visual Studio Code com extensão C#

### Instalação

1. Clone o repositório:
```bash
git clone https://github.com/JoaoQueirozDev27/Gerador-De-Videos.git
```

2. Navegue até o diretório do projeto:
```bash
cd Gerador-De-Videos
```

3. Mude para a branch `versao-nova`:
```bash
git checkout versao-nova
```

4. Restaure as dependências:
```bash
dotnet restore
```

5. Compile o projeto:
```bash
dotnet build
```

## 🔧 Uso

O projeto possui **3 apresentações principais** (além do Drag and Drop), cada uma com um propósito específico:

### 1️⃣ **G1Videos** - Processamento Automático de Notícias
**Localização:** `GeradorVideosFinal/Core/G1Videos/`

**Propósito:** Gera vídeos automaticamente a partir de conteúdo web (notícias).

**Como usar:**
1. Execute o projeto G1Videos
2. O sistema realiza web scraping automaticamente para obter notícias
3. Para cada notícia:
   - Gera um roteiro estruturado via IA
   - Separa em: **Título** + **Conteúdo principal**
   - Cria arquivo de texto com o conteúdo resumido
   - Gera áudio via TTS (Text-to-Speech)
   - Processa o vídeo com sincronização de áudio

**Fluxo:**
```
Buscar notícias → Gerar roteiro (IA) → Extrair dados → Gerar áudio → Processar vídeo
```

**Saída:** Pastas organizadas (ex: `C:\Desktop\G1\Video0`, `Video1`, etc.)

---

### 2️⃣ **TesterConsole** - Testes com Temas Pré-definidos
**Localização:** `GeradorVideosFinal/Core/TesterConsole/`

**Propósito:** Teste rápido de funcionalidades com temas humorísticos pré-configurados.

**Como usar:**
1. Execute o projeto TesterConsole
2. O sistema processa automaticamente uma lista de temas engraçados
3. Temas incluem:
   - "Primeiro encontro que deu errado"
   - "Entrevista de emprego que virou trauma"
   - "Tentando cozinhar sem saber fritar um ovo"
   - E mais 17 temas divertidos...

**Fluxo:**
```
Tema pré-definido → Gerar mapa de conteúdo (IA) → Gerar áudio (TTS) → Processar vídeo
```

**Ideal para:** Testes, prototipagem rápida e demonstrações

---

### 3️⃣ **Teste2Abstração** - Interface Interativa Flexível ⭐
**Localização:** `GeradorVideosFinal/Core/Teste2Abstração/`

**Propósito:** Controle total com menu interativo para criar e renderizar modelos de vídeo.

**Como usar:**

**Opção 1 - Renderizar um Modelo:**
1. Execute o projeto
2. Selecione "Renderizar um modelo"
3. Escolha um arquivo `.json` da pasta `C:\Users\Administrador\Desktop\Modelos`
4. O sistema carrega e exibe:
   - ID do modelo
   - Resolução de saída
   - Status do áudio
   - Prompt utilizado
5. Se o prompt contiver variáveis dinâmicas (`/*nome*/`), o sistema solicita valores:
   ```
   Digite o valor para a variável [bold]assunto[/]: Tecnologia
   ```
6. Processa e gera o vídeo com os parâmetros definidos

**Opção 2 - Criar um Modelo:**
1. Selecione "Criar um modelo"
2. Configure:
   - Nome do template
   - Resolução desejada
   - Ativar/desativar áudio principal
   - Prompt com IA
   - Cenas adicionais (opcional)
3. Salva automaticamente em `.json`

**Recursos especiais:**
- ✅ **Variáveis dinâmicas** nos prompts: `/*nome_variavel*/`
- ✅ Configuração visual de resolução e áudio
- ✅ Sistema de cenas para organizar conteúdo
- ✅ Interface amigável com Spectre.Console (menus e seleções)
- ✅ Persistência em modelos reutilizáveis

**Exemplo de uso avançado:**
```
Criar modelo:
  Nome: "Vídeo Educativo"
  Resolução: 1920x1080
  Áudio: Ativado
  Prompt: "Crie um vídeo sobre /*assunto*/ em tom /*tom*/"
  
Renderizar:
  Sistema solicita: "Qual é o assunto? Inteligência Artificial"
  Sistema solicita: "Qual é o tom? Educativo e envolvente"
```

---

### 📊 Comparativo das Presentations

| Feature | G1Videos | TesterConsole | Teste2Abstração |
|---------|----------|---------------|-----------------|
| **Fonte de dados** | Web Scraping | Temas fixos | Menu interativo |
| **Flexibilidade** | Baixa (automático) | Baixa (pré-definido) | **Alta** ✨ |
| **Variáveis dinâmicas** | ❌ | ❌ | ✅ |
| **Modelos JSON** | ❌ | ❌ | ✅ |
| **Interação do usuário** | Nenhuma | Nenhuma | Completa |
| **Caso de uso** | Produção automática | Prototipagem | Desenvolvimento flexível |

---

## 📂 Estrutura do Projeto

```
GeradorVideosFinal/Core/
├── Application/          # Lógica de aplicação e Use Cases
├── Services/             # Serviços principais (IA, Áudio, Vídeo)
├── Persistence/          # Persistência de dados
├── Elements/             # Elementos reutilizáveis
├── Assets/               # Recursos do projeto
├── G1Videos/             # [PRESENTATION] Processamento de notícias
├── TesterConsole/        # [PRESENTATION] Testes com temas pré-definidos
├── Teste2Abstração/      # [PRESENTATION] Interface interativa
└── TesteDragAndDrop/     # [PRESENTATION] Drag and Drop (experimental)
```

**Serviços principais:**
- `IAiService` - Integração com IA para geração de conteúdo
- `IAudioService` - Text-to-Speech e processamento de áudio
- `IVideoService` - Processamento e criação de vídeos
- `IMediaManager` - Gerenciamento de mídia (YouTube, etc)

---

## 🤝 Contribuindo

Para contribuir com este projeto:

1. Crie uma nova branch a partir de `versao-nova`
2. Commit suas mudanças
3. Faça um push para sua branch
4. Abra um Pull Request

---

## 📝 Licença

[Especifique a licença do projeto, se houver]

## 👨‍💻 Autor

Desenvolvido por [JoaoQueirozDev27](https://github.com/JoaoQueirozDev27)

## 📧 Contato

Para dúvidas ou sugestões, entre em contato através do GitHub.
