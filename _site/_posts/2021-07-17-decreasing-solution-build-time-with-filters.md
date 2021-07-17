---
title: 'Decreasing Solution Build time with Filters'
description: ''
cover_image: '/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/cover_image.png'
image: '/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/cover_image.png'
tags:
  - "Visual Studio"
  - "MSBuid"
---

Body...



Do you ever have the feeling that you are late to the party when discovering a feature?




You have probably heard that [Visual Studio 2022](https://devblogs.microsoft.com/visualstudio/visual-studio-2022-preview-1-now-available) is around the corner. This Visual Studio release moves from the 32-bit to the 64-bit architecture which paves the way for performances improvements. 

I am a big fan of the Single Solution Model for managing my projects. 

In my projects both personal and professional I prefer to use the so called Single Solution Model. It offers benefits like a single entry point to contribute to the project






- https://github.com/dotnet/msbuild/issues/4097

- https://dailydotnettips.com/selective-projects-loading-using-solution-filter-in-visual-studio/
- https://www.michalbialecki.com/2019/11/16/visual-studio-has-now-solution-filtering/
- https://davecallan.com/filtering-projects-large-solutions-visual-studio-solution-filters/


- https://developercommunity.visualstudio.com/t/solution-build-filter-as-opposed-to-solution-filte/789064
- https://github.com/NuGet/Home/issues/5820
- https://github.com/dotnet/sdk/issues/10409
- https://dailydotnettips.com/selective-projects-loading-using-solution-filter-in-visual-studio/

- https://developercommunity.visualstudio.com/t/allow-save-as-new-solution-filter-from-existing-so/515388

https://www.jetbrains.com/help/rider/Managing_Projects_and_Solutions.html

https://andrewlock.net/creating-and-editing-solution-files-with-the-net-cli/

There are many ways to structure your projects source code. My preference is a style called single-solution-model. Amongst other things, I like that it provides a single entry point to my project. If, however, your project grows, it can become slow to build it. I am sure some of you will be familiar with the following [xkcd joke](https://imgs.xkcd.com/comics/compiling.png) or some variant of it:

![xkcd_joke](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/xkcd_joke_compiling.png)

The [next version](https://devblogs.microsoft.com/visualstudio/visual-studio-2022-preview-1-now-available) of Visual Studio will come with a lot of promised performance improvements. VisualStudio 2022 is the first version that takes advantage of the 64-bit processor architecture. I have not yet tested it, but I am hopeful for a more performant experience developing when it ships.

> While I think the 1600+ projects in a solution demo are cool, I would not see myself using the single solution model at that scale.

That brings me to the topic of today's post. I recently discovered a VS2019 feature I did not know that can bring some improvement to my experience. VS2019 introduced a new feature called [solution filters](https://docs.microsoft.com/en-us/visualstudio/ide/filtered-solutions?view=vs-2019).

## Project Setup

I think over my past couple of posts; it's become clear that I am a fan of the `Microsoft.Extensions` repository. While Microsoft uses multiple solution files throughout the repository, I would opt for the single solution model.

Many of the projects in the repo follow this pattern:

- Concept.Abstractions provides interfaces
- Concept provides default implementation for Concept.Abstractions
- Concept.Concrete technology specific implementation for Concept.Abstractions

## Setting up our filters

After following these steps, our project should look like the picture below in Visual Studio.

![sln-all-projects](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/001_vs2019_sln_all_projects.png)

We can select one or more projects at a time and unload them from the solution.

![sln_unload_projects](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/002_vs2019_sln_unload_projects.png)

Up until now, this is how I would have done things. Just unload projects I won't need and don't worry about them anymore. What I did not know is that we save the current state of the solution.

![sln_save_filter_001](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/003_vs2019_sln_save_filter_001.png)

Unloading projects manually to create filters can be error-prone. Since a solution filter only builds the projects selected by the filter missing a project causes the build to fail.

An alternative can be to unload all projects, select the project you want, and use the "reload with dependencies" option.

![sln_reload_project_dependencies](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/004_vs2019_sln_reload_project_dependencies.png)

Like before, we can save the solution filter with the "Save As Solution Filter" option. The only difference is that we now get 4/7 projects as opposed to 5/7 projects. That's because we loaded the "ConcreteBravo.Tests" projects and it's dependencies. Even though that loads "Extensions.Concept" it does not load "Extensions.Concept.Tests" since it is not a dependency of "ConcreteBravo.Tests".

![sln_save_filter_002](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/005_vs2019_sln_save_filter_002.png)

While researching something unrelated to this post, I noticed that the [EF Core team](https://github.com/dotnet/efcore) used this feature I did not even know existed. The cool thing was that they also had a filter for all projects. So I had to try that out, and as it turns out, you can create a filter without unloading projects.

![sln_save_filter_003](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/006_vs2019_sln_save_filter_003.png)

The image below shows the difference between the three filters we created. It looks exactly like a traditional Solution Explorer with the addition that the name of the filter applied is displayed.

![slnf_project_overview](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/007_vs2019_slnf_project_overview.png)

## Manage solution changes

You might be wondering what happens when I need to add new projects to my solution?

To demonstrate, let us assume our test projects have a shared helper project. At this time, I want to update our "Concept.Bravo" solution filter. This time I don't want to use dotnet CLI but use "Add existing project".

![slnf_add_existing_project](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/008_vs2019_slnf_add_existing_project.png)

As soon as you did this, you get this pop-up stating a mismatch between the loaded projects and the project specified in the filter.

If you followed the steps in a GIT environment, you would see that even before pressing " Update Solution Filter" the underlying solution is already updated.

![slnf_update_solution_filter](/assets/images/posts/20210717/decreasing-solution-build-time-with-filters/009_vs2019_slnf_update_solution_filter.png)

## The missing bit

I discussed this feature at work as a potential workaround for an issue we had in structuring our projects. One of my colleagues remembered looking at it about a year ago and finding it lacking. A few minutes later, he found a [post](https://developercommunity.visualstudio.com/t/Solution-Filter-should-allow-for-Include/1090914?space=8&q=solution+filter) on the developer community for Visual Studio. Funnily enough, it's a small world; the user-post links to a GitHub issue he created in this matter.

The problem is the management of multiple solutions filters because the filters are inclusive with relative paths following the sln-filter location. A proposed improvement would be to use glob patterns to include/exclude projects. That would make it easier when following naming conventions to have always up-to-date filters.

At a customer I work for, they use PowerShell as their script platform of choice, so I needed a deeper understanding of PowerShell. With PowerShell, it's reasonably easy to work with the file system and convert from and to JSON. So I thought, how hard can it be to script this.

The following script loads the paths of all *.csproj present in the solution directory and filters them out by RegEx. It then writes it to disk in the .slnf-format.

```ps
$todo
```

## Closing Thoughts


As always, if you have any questions, feel free to reach out. Do you have suggestions or alternatives? I would love to hear about them.

I like this new feature as a way to manage my larger solutions. Of course, it's not practical to maintain my (very basic) script for this. It will be a huge help if you think this is a valuable feature to upvote the issue at the Visual Studio Community forum.

The corresponding source code for this article is on [GitHub](https://github.com/kaylumah/GenerateCSharpClientForOpenAPI).

See you next time, stay healthy and happy coding to all 🧸!


## Todos

- [] build CLI
- [] update filter with dotnet cli

## Sources

- https://docs.microsoft.com/en-us/visualstudio/ide/filtered-solutions?view=vs-2019
- https://docs.microsoft.com/en-us/visualstudio/msbuild/solution-filters?view=vs-2019