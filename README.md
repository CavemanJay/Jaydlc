# Jay's Personal Website

## What is on this site?

- Whatever I feel is worth throwing on there
- Hacking/ctf writeups
- Gaming clips

## What is the purpose of this site?

- Build something of my own
- Show off some of my knowledge
- Justify the purchase of my domain
- Incorporate new tech into a project for the fun of it (GraphQL, Blazor)

## What tech does is this site built off of?

- .NET
    - C#
    - .NET 5
    - Blazor Server

## Why Blazor?

- C# was the first language I got actual programming experience with (setting aside VBA)
    - Introduced to me by Omar over at http://omarvision.com/
- I prefer statically typed languages like go, c#, f# over python
    - So that's why not flask/django

- I have experience with MVC as well as SPAs (react/angular)
    - Blazor seemed like a good middle ground. You can use components like SPAs and Flutter as well as change data
      without refreshing the entire page but all of the C# code is server side so I can still do database calls or IO
      operations without having to go through an API

## What projects does this site depend on?

- [Hot Chocolate (GraphQL)](https://github.com/ChilliCream/hotchocolate)
- [.NET 5](https://dotnet.microsoft.com/)
- Linux
    - My current server is running Arch
- [Youtube-dl](https://youtube-dl.org/)
    - For getting the data about my own videos that I upload without having to go through the youtube api

- [git](https://git-scm.com/)

## Things I still want to add

- Auto updating module
    - Possible implementations
        - Github webhook listener watching specific branch
         
        - GRPC service that checks the authentication header against some environment variable
            - Upload tar or zip of website contents
            - Extract archive to some specified folder (timestamped?)
            - Either start an exe/script with a delay (forking/disowning) and then shutdown current process
            - Have external script startup the website in the proper folder without ownership of the process

        - Website runs under some sort of controller process that can kill the site and replace with new version
            - Some sort of custom exe? Or maybe some linux program already exists that can do this
    
- Some sort of way to browse my projects on github
- Website changelog?