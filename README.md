## UiPath fork of FreeRDP

This repo forks the FreeRDP repo, thus allowing us to make changes that fit our needs and augment the codebase with other components.
From time to time there is a need to merge the changes from the original repo into this one.

### ❗When updating the FreeRDP library from the official repo, please update the following table in this file (README.md):

|**Original repository tag/branch used:**| `2.11.2` |
| --- | --- |
|**Original corresponding commit hash:**| `a38c1be9eee39a9bc22b511fffe96e63fdf8ebe7` |

#### Updating the fork is done on a dev machine and involves rebasing our changes on top of the new tag/branch from the original repo and **force pushing** the `uipath` branch.

❗The state before the update must be saved in a support branch, named `robot/support/before_update_to_<NEW BASE GIT TAG>`, where `<NEW BASE GIT TAG>` is the tag from the original repo onto which we'll rebase our changes.

The tags from the original repo must be fetched from the original repo and pushed to our fork:
```pwsh
git remote add upstream https://github.com/FreeRDP/FreeRDP.git
git fetch --tags --all
git push --tags
```

A work branch should be created from `uipath` and used in the validation phase. Its name would be `feat/update_to_<NEW BASE GIT TAG>`.
After checking out the branch, the rebase should be performed using the following command:
```pwsh
git rebase --onto <NEW BASE Git Tag> <Our 1st customization Commit's Hash> <The work branch>
```
In this phase we have the opportunity the review the new state, run CI/CD and fix any issues that might arise.

After validating the `feat/update_to_<NEW BASE GIT TAG>` branch, reset the `uipath` branch to it and force push it.

### Build instructions
* Visual Studio 2022 installed in `C:\Program Files` required.

* Install [StrawberryPerl](http://strawberryperl.com).  Make sure the `perl` command is in PATH.
  You may try on newer Windows 10:
```
    winget install -e --id StrawberryPerl.StrawberryPerl
```

#### Build FreeRDP and Build OpenSSL (dependency for FreeRDP)

> Use a developer console for VS 2022 instead of normal PowerShell or CMD, the commands require `nmake`

* Steps
  * Clone [OpenSSL](https://github.com/openssl/openssl) to `..\openssl` && Checkout tag `OpenSSL_1_0_2u` (getOpenSsl)
  * Generate OpenSSL build to `..\OpenSSL-VC-64`.  (buildOpenSsl)
  * Use CMake to generate and then build Visual Studio 2022 solutions.  (BuildFreeRDP)
  * The freerdp solution is generated in `.\Build\x64\` directories.

* Scripts
  * Simple
    ```
    cd .ci/Scripts
    .\PrepareFreeRdpDev
    ```
  * or detailed
    ```
    cd .ci/Scripts
    .\getOpenSsl
    .\buildOpenSsl
    .\buildFreeRDP Debug
    ```

### Work on the FreeRdpClient
* Open [UiPath.FreeRdpClient/UiPath.FreeRdpClient.sln](file://UiPath.FreeRdpClient/UiPath.FreeRdpClient.sln)
* To test with a nugetRef instead of projectRef edit the [UiPath.FreeRdp.Tests.csproj](file://UiPath.FreeRdpClient/UiPath.FreeRdpClient.Tests/UiPath.FreeRdp.Tests.csproj)
search for: `<When Condition="'$(UseNugetRef)'!=''">`

#### Running unit tests

* Run Visual Studio as Admin
* Make sure RDP is enabled on local machine - `View advanced system settings > Remote tab > Allow remote connections to this computer`
