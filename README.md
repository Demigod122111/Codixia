# Codixia & CodixiaUI
Codixia and CodixiaUI are lightweight C# frameworks built on top of Raylib_cs, designed to simplify and accelerate game development while keeping full control and performance.

They provide a clean structure, modern patterns, and reusable systems so you can focus on gameplay rather than boilerplate.

## Features
### Codixia

A core game framework that extends raylib with:

+ Structured game loop and lifecycle management
+ Scene and state management
+ Designed for clarity, performance, and extensibility
+ ...and more!

### CodixiaUI

A UI framework built on top of Raylib_cs, focused on flexibility and correctness:
+ Hierarchical UI system (UIElement, UIContainer)
+ Proper input propagation (top-most first then serial)
+ Mouse filtering (Ignore, Pass, Stop)
+ Visibility and enable/disable handling
+ Custom drawing and layout control
+ Engine-agnostic UI logic, tightly integrated with raylib rendering

## 🎯 Goals

1. Reduce boilerplate when using raylib_cs
2. Keep the framework simple and readable
3. Avoid unnecessary abstraction or engine lock-in
4. Make small-to-medium C# games faster to build

## 📦 Built On

C# - .NET Core 9.0

Raylib_cs – the official C# bindings for raylib

No external UI libraries, no heavy dependencies.

## 🚀 Getting Started

Clone the repository:

1. git clone https://github.com/Demigod122111/Codixia.git
2. Reference Codixia (and CodixiaUI if needed) in your project.
3. Initialize raylib normally, then let Codixia handle structure and flow.


## 🛠 Status

Codixia and CodixiaUI are actively developed and evolving.
APIs may change as systems are refined and improved.

Feedback, issues, and contributions are welcome.