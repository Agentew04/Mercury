# Mercury

* Ler em [português](README.pt.md).

## What is Mercury?

Mercury is a flexible framework for instruction-accurate computer simulation.
Uses C# as a Domain Specific Language(DSL). Enables description of any<sup>*</sup> ISA
and organizes a computer into different modules, decreasing the amount of work needed
to add execution support to a ISA.

<sup>*</sup>Currently only 32 fixed width instructions are supported.

> [!NOTE]
> This project is product of my graduation thesis. Available [here](https://repositorio.ufsm.br/bitstream/handle/1/37299/Appelt_Rodrigo_2025_TCC.pdf).

## Engine

The Engine is a collection of multiple simulators capable of simulating an 
entire computer. Uses a modular and extensible architecture, allowing for easy
implementation of new architectures and simulators. Also, currently supports any static
ELF executable, allowing students to run real-world programs.

Architectures currently supported:
* **MIPS:** the entire MIPS-I ISA is supported, with support for some instructions
found in later revisions of the ISA.
* **RISC-V:** not currently supported. Planned for future releases after support
for MIPS is stable.

## Editor

The Editor is an IDE that interfaces with the simulators provided by the Engine.
It provides a user-friendly interface for students to write, compile, and run
assembly code. Also has a integrated quick guide of basic concepts of computer
architecture and assembly programming.

TODO: add screenshots

## Installation

TODO: add installation instructions