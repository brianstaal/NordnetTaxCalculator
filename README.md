# NordnetTaxCalculator
Just a quick and dirty tax calculator for your stocks @ Nordnet

Description
===========
It's a very simple dotnet 10 console application that reads a CSV export from Nordnet

WARNING: For now it is NOT working, but in a few days it might be.

Usage
=====
1. Clone the repository
2. Build the project using `dotnet build`
3. Create a folder named AppData in the project root and place your Nordnet CSV file there
4. Run the application with `dotnet run`

Roadmap
=======
- [x] Parse CSV from Nordnet
- [ ] Calculate gains/losses per stock
- [ ] Calculate remaining holdings
- [ ] Generate tax report per year using first-in-first-out (FIFO) method
- [ ] Documentation

Changelog
=========
v0.1 - Initial release (27.01.2026)