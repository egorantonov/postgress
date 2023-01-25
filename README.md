# Postgress
![pg256](https://user-images.githubusercontent.com/16327320/214688624-6a2d2fe5-0096-4b44-bf31-66ce36b49186.jpg)

aka `The Simplest Browser Geoloc` game automation tool

## What is it?
### tldr;
Another pet project.
### But really
This is a console application which automate gaming process due to game API vulnerabilities.
You can _move_, <strike>hack</strike> discover <strike>portals</strike> points and deploy <strike>resonators</strike> cores.
In the near future other features such as upgrade, <strike>recharge</strike> repair and attack will be implemented.
This is gonna be alive until developers of the original app decide to move crucial validations from the client to the server (the way <a href="https://ingress.com/game/">Ingress</a> works).

## Features
### Now you can:
- Move (virtually by setting active rectangular area)
- Discover points
- Deploy 

### But you can't:
- Repair points
- Upgrade cores
- Attack enemy points

## How to use
Currently you need to change some code to move, so you definitely need an IDE 😀

Ok, these are basic steps:

0. <a href="https://3d.sytes.net:8080/login">Register or login</a> in the game, copy your username and auth token
1. Open `Postgress.sln` solution
2. Change `UserName`, `UserTeam` and `Token` in `Constants.cs` (or play as `JARVIS`)
2. To move change area borders (`south`, `north`, `west`, `east`) in `Program.cs`
3. Build and launch an app
4. Enjoy

## Credits
<a href="https://3d.sytes.net:8080/">The Simplest Browser Geoloc</a>
