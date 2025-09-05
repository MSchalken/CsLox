# CsLox

A C# implementation of the **Lox** programming language interpreter from the book *Crafting Interpreters* by Robert Nystrom.  
This project covers the **tree-walking interpreter** from Part I of the book, implemented in idiomatic C#.

---

## ✨ Features
- Full implementation of the Lox language as described in Part I:
  - Variables, control flow (`if`, `while`, `for`)
  - Functions and closures
  - Classes and inheritance
- REPL (interactive prompt) and script execution modes
- Error reporting for syntax and runtime errors
- Based on the **recursive descent parser** and **AST interpreter** architecture

---

## 🛠 Tech Stack
- **Language:** C# (tested on .NET 8+)
- **Paradigm:** Object-oriented, tree-walking interpreter
- **Book Reference:** *Crafting Interpreters* (Part I)

---

## ▶️ Getting Started

### **Prerequisites**
- .NET SDK 8.0+

### **Build & Run**
```bash
# Clone the repository
git clone https://github.com/mschalken/CsLox.git
cd CsLox
```

### Build
```bash
dotnet build
```

### Run REPL
```bash
dotnet run
```

### Run a Lox script
```bash
dotnet run -- path/to/script.lox
```

## 📚 References
- Crafting Interpreters by Robert Nystrom
