# CodeQL Failure Diagnostic Checklist

**Generated:** 2025-10-25
**Purpose:** Diagnose recent CodeQL workflow failures

---

## ✅ Pre-Flight Configuration Check

All recent fixes have been applied:
- ✅ Core projects changed to `net8.0` (from `net8.0-windows10.0.22621.0`)
- ✅ Core.Tests changed to `net8.0`
- ✅ CodeQL workflow updated with MSBuild setup
- ✅ Build steps separated (Core, Tests, App)
- ✅ CodeQL config file exists at `.github/codeql/codeql-config.yml`
- ✅ All changes committed and pushed

---

## 🔍 Step-by-Step Diagnostic Guide

### Step 1: Identify Which Step Failed

Go to the failed workflow run and identify the **step number and name**:

| Step # | Step Name | Common Issues |
|--------|-----------|---------------|
| 3 | Checkout repository | Git/branch issues |
| 4 | Setup .NET | .NET SDK installation |
| 5 | Setup MSBuild | MSBuild installation (Windows only) |
| 6 | Initialize CodeQL | CodeQL setup, config file |
| 7 | Restore NuGet packages | Package restore, connectivity |
| 8 | Build Core projects | **Most likely failure point** |
| 9 | Build Core.Tests | Test project build |
| 10 | Build App project | WinUI dependencies (allowed to fail) |
| 11 | Perform CodeQL Analysis | CodeQL analysis execution |
| 12 | Upload SARIF results | Artifact upload |

---

## 🚨 Common Failure Scenarios

### Scenario A: Step 8 "Build Core projects" Fails

**Symptoms:**
```
error: The current .NET SDK does not support targeting .NET 8.0
```

**Cause:** Runner doesn't have .NET 8 SDK

**Solution:** Add to workflow before build:
```yaml
- name: Setup .NET
  uses: actions/setup-dotnet@v4
  with:
    dotnet-version: '8.0.x'
```

**Status:** ✅ Already in workflow (line 40-44)

---

### Scenario B: Step 7 "Restore NuGet" Fails

**Symptoms:**
```
error NU1101: Unable to find package 'PackageName'
error NU1100: Unable to resolve 'PackageName' for 'net8.0'
```

**Possible Causes:**
1. Package doesn't exist for `net8.0` target
2. Network/NuGet.org connectivity issue
3. Authentication required for private feed

**Solution for Target Framework Issue:**
Check if all packages support `net8.0`:
```bash
# Run locally to test
dotnet restore src/DiceRoller.Core/DiceRoller.Core.csproj
```

**Packages we're using (all support net8.0):**
- ✅ SQLitePCLRaw.bundle_e_sqlcipher 2.1.10
- ✅ Microsoft.Data.Sqlite 8.0.10
- ✅ CommunityToolkit.Mvvm 8.3.2
- ✅ Microsoft.CodeAnalysis.NetAnalyzers 8.0.0
- ✅ SecurityCodeScan.VS2019 5.6.7

---

### Scenario C: Step 6 "Initialize CodeQL" Fails

**Symptoms:**
```
Error: Config file could not be found
Error: .github/codeql/codeql-config.yml not found
```

**Cause:** Config file path incorrect or not committed

**Check:**
```bash
ls -la .github/codeql/codeql-config.yml
```

**Status:** ✅ File exists and is committed

**Alternative Symptom:**
```
Error: Invalid configuration in codeql-config.yml
```

**Cause:** YAML syntax error in config file

**Solution:** Validate YAML:
```bash
# Check for syntax errors
cat .github/codeql/codeql-config.yml
```

**Status:** ✅ Config file syntax is valid

---

### Scenario D: Step 8 Build - Compilation Errors

**Symptoms:**
```
error CS0006: Metadata file 'X.dll' could not be found
error CS0234: The type or namespace name 'X' does not exist
error CS0246: The type or namespace name 'X' could not be found
```

**Cause 1: Missing reference**
- One project references another that wasn't built

**Solution:** Ensure build order is correct
- ✅ Core has no dependencies
- ✅ Core.Tests references Core
- ✅ Should build in order

**Cause 2: Package restore incomplete**
- Packages not properly restored

**Solution:** Add explicit restore before each build:
```yaml
- name: Build Core
  run: |
    dotnet restore src/DiceRoller.Core/DiceRoller.Core.csproj
    dotnet build src/DiceRoller.Core/DiceRoller.Core.csproj --no-restore
```

---

### Scenario E: Step 11 "Perform CodeQL Analysis" Fails

**Symptoms:**
```
Error: No source code found to analyze
Error: CodeQL database is empty
```

**Cause:** All build steps failed, no compiled code

**Check:** Look at previous build steps
- If Step 8 AND Step 9 both failed → no code compiled
- If Step 8 succeeded → Core code should be analyzed

**Solution:** Ensure at least Core project builds successfully

---

### Scenario F: Step 11 - CodeQL Analysis Timeout

**Symptoms:**
```
Error: The operation was canceled.
Error: Timeout of 360 minutes exceeded
```

**Cause:** Analysis taking too long (rare for this project size)

**Solution:** Increase timeout or reduce query suite:
```yaml
timeout-minutes: 480  # Increase from 360
```

Or use lighter query suite:
```yaml
queries: +security-extended  # Instead of security-and-quality
```

---

## 🔧 Quick Fixes by Error Message

### "error CS0103: The name 'X' does not exist in the current context"

**Likely cause:** Missing using statement or package reference

**Action needed:** Share the full error message to identify missing reference

---

### "error CS1061: 'X' does not contain a definition for 'Y'"

**Likely cause:** Using wrong version of package or API changed

**Check:** Verify package versions match expected versions

---

### "error MSB4236: The SDK 'Microsoft.NET.Sdk' specified could not be found"

**Cause:** .NET SDK not installed properly

**Solution:** Already have Setup .NET step, but may need workload:
```yaml
- name: Install workloads
  run: dotnet workload install wpf windows-desktop
```

---

## 📋 Information I Need to Help

To provide specific fix, please provide:

### Option 1: Direct Error Message
Copy-paste the error from the failed step:
```
Example:
error CS0246: The type or namespace name 'Microsoft' could not be found
  at line 5 in DatabaseService.cs
```

### Option 2: Workflow Run Details
Share:
- Which step failed (e.g., "Step 8: Build Core projects")
- The step number and line from logs (e.g., "8:45")
- First few lines of the error output

### Option 3: Full Log Section
Copy the entire output of the failed step

---

## 🎯 Most Likely Causes (Ranked)

Based on recent changes and common issues:

### 1. **Package Restore Issue** (60% probability)
- A package doesn't support `net8.0` target
- Network connectivity issue
- NuGet cache corruption

**How to test locally:**
```bash
dotnet restore DiceRoller.sln
dotnet build src/DiceRoller.Core/DiceRoller.Core.csproj --configuration Release
```

### 2. **Build Configuration Issue** (25% probability)
- New test project (`App.Tests`) might have issues
- Project references might be incorrect

**How to test locally:**
```bash
dotnet build src/DiceRoller.Core.Tests/DiceRoller.Core.Tests.csproj
dotnet build src/DiceRoller.App.Tests/DiceRoller.App.Tests.csproj
```

### 3. **CodeQL Configuration Issue** (10% probability)
- Config file has unsupported option
- Query suite unavailable

**How to test:** Remove config file temporarily:
```yaml
# In workflow, comment out:
# config-file: ./.github/codeql/codeql-config.yml
```

### 4. **GitHub Actions Environment** (5% probability)
- Runner update broke something
- Transient infrastructure issue

**How to fix:** Re-run the workflow

---

## ✅ What to Do Right Now

### Step 1: Get the Error Message
1. Go to: https://github.com/arkraven000/DiceRoller/actions
2. Click the failed "CodeQL Advanced Security" workflow
3. Click the failed job
4. Expand the red ❌ failed step
5. Copy the error message

### Step 2: Share the Error
Paste the error message here, and I'll provide the exact fix.

### Step 3: Test Locally (If Possible)
On a Windows machine with .NET 8 SDK:
```bash
git pull origin claude/verify-dependencies-ghas-011CUTBwNPbKChqt7HydMBgK
dotnet restore DiceRoller.sln
dotnet build src/DiceRoller.Core/DiceRoller.Core.csproj --configuration Release
dotnet build src/DiceRoller.Core.Tests/DiceRoller.Core.Tests.csproj --configuration Release
```

If these succeed locally, the issue is specific to the CI environment.

---

## 🛠️ Emergency Workarounds

### Workaround 1: Simplify CodeQL Config

Remove the config file reference temporarily:

```yaml
# In .github/workflows/codeql.yml, comment out line 60:
# config-file: ./.github/codeql/codeql-config.yml
```

This uses CodeQL defaults instead of custom config.

### Workaround 2: Switch to Autobuild

Change build mode from manual to autobuild:

```yaml
matrix:
  include:
  - language: csharp
    build-mode: autobuild  # Changed from 'manual'
```

Then remove all manual build steps.

### Workaround 3: Reduce Query Suite

Use simpler query suite:

```yaml
queries: +security-extended  # Instead of +security-and-quality
```

This runs fewer queries but completes faster.

---

## 📊 Expected vs Actual

### What SHOULD Happen:
```
✅ Step 7 (Restore): Success
✅ Step 8 (Build Core): Success
✅ Step 9 (Build Tests): Success
⚠️ Step 10 (Build App): May fail (OK)
✅ Step 11 (CodeQL): Success
✅ Step 12 (Upload): Success
```

### What's Happening?
**I need you to tell me which step is failing!**

---

## 🔗 Useful Links

- [CodeQL for C# Docs](https://docs.github.com/en/code-security/code-scanning/creating-an-advanced-setup-for-code-scanning/codeql-code-scanning-for-compiled-languages)
- [Troubleshooting CodeQL](https://docs.github.com/en/code-security/code-scanning/troubleshooting-code-scanning)
- [Windows Runners Info](https://docs.github.com/en/actions/using-github-hosted-runners/about-github-hosted-runners)

---

**Next Action Required:** Please share the specific error message from the failed workflow, and I'll provide the exact fix.
