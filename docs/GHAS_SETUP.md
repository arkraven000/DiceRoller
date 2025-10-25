# GitHub Advanced Security (GHAS) Setup Guide

This document provides information about the GitHub Advanced Security configuration for the DiceRoller project.

## Overview

GitHub Advanced Security (GHAS) provides advanced security features to help identify and fix security vulnerabilities in your code and dependencies.

## Features Enabled

### 1. CodeQL Analysis

**File:** `.github/workflows/codeql.yml`

CodeQL is GitHub's semantic code analysis engine that finds security vulnerabilities before they reach production.

**Features:**
- Automated code scanning for security vulnerabilities
- Runs on every push, pull request, and weekly schedule
- Scans C# code with security-and-quality query suite
- Results available in Security tab > Code scanning alerts

**Languages Analyzed:**
- C# (.NET 8.0)

**Query Suites:**
- `security-and-quality`: Comprehensive security and code quality checks

**Schedule:**
- Triggered on: Push to main/develop branches
- Pull requests to main/develop
- Weekly on Mondays at 3 AM UTC
- Manual dispatch available

**Configuration File:** `.github/codeql/codeql-config.yml`

### 2. Dependabot

**File:** `.github/dependabot.yml`

Dependabot automatically checks for outdated dependencies and security vulnerabilities.

**Features:**
- Automated dependency updates
- Security vulnerability alerts
- Automatic pull requests for updates
- Grouped updates for related packages

**Package Ecosystems Monitored:**
1. **NuGet Packages** (for .NET dependencies)
   - Weekly updates on Mondays at 4 AM UTC
   - Groups: Microsoft, CommunityToolkit, Testing, Security Analyzers

2. **GitHub Actions** (for workflow dependencies)
   - Weekly updates on Mondays at 4 AM UTC
   - All actions grouped together

**Update Groups:**
- `microsoft-dependencies`: All Microsoft.* packages
- `communitytoolkit-dependencies`: All CommunityToolkit.* packages
- `testing-dependencies`: xunit, FluentAssertions, Moq, coverlet
- `security-analyzers`: SecurityCodeScan, Microsoft.CodeAnalysis

### 3. Secret Scanning

**File:** `.github/workflows/security-scan.yml` (already exists)

Uses TruffleHog to detect hardcoded secrets in the codebase.

### 4. Dependency Review

Automatically enabled for pull requests when Dependabot is configured.

**Features:**
- Reviews dependency changes in PRs
- Identifies vulnerable dependencies
- Provides security advisories
- Blocks PRs with vulnerable dependencies (if configured)

## Security Scanning Results

### Where to Find Results

1. **CodeQL Alerts**
   - Navigate to: **Security** tab → **Code scanning**
   - View alerts by severity, type, and status
   - Each alert includes remediation guidance

2. **Dependabot Alerts**
   - Navigate to: **Security** tab → **Dependabot**
   - View vulnerable dependencies
   - Click alerts for CVE details and fix recommendations

3. **Secret Scanning Alerts** (if enabled)
   - Navigate to: **Security** tab → **Secret scanning**
   - View detected secrets
   - Revoke and rotate exposed secrets

## Configuration Files

```
.github/
├── workflows/
│   ├── codeql.yml                 # CodeQL analysis workflow
│   ├── build.yml                  # Build and test workflow
│   └── security-scan.yml          # Additional security scanning
├── dependabot.yml                 # Dependabot configuration
└── codeql/
    └── codeql-config.yml          # CodeQL custom configuration
```

## Enabling GHAS Features

### Prerequisites

- GitHub Advanced Security must be enabled for the repository
- Repository must be public OR have GHAS license for private repos

### Steps to Enable

1. **Enable Code Scanning**
   ```
   Repository Settings → Code security and analysis → Code scanning → Enable
   ```
   The workflow file `.github/workflows/codeql.yml` will handle the rest.

2. **Enable Dependabot**
   - Dependabot alerts: Auto-enabled for public repos
   - Dependabot security updates: Enable in repository settings
   - Dependabot version updates: Auto-enabled via `dependabot.yml`

3. **Enable Secret Scanning**
   ```
   Repository Settings → Code security and analysis → Secret scanning → Enable
   ```

## Workflow Triggers

### CodeQL Analysis
- Push to `main`, `develop`, `claude/**` branches
- Pull requests to `main`, `develop`
- Weekly schedule: Mondays at 3 AM UTC
- Manual: Repository → Actions → CodeQL → Run workflow

### Dependabot
- Automatic: Checks daily for security updates
- Scheduled: Weekly on Mondays at 4 AM UTC for version updates

### Security Scan
- Push to `main`, `develop` branches
- Pull requests to `main`, `develop`
- Daily schedule: 2 AM UTC

## Managing Alerts

### CodeQL Alerts

1. **Review Alert**
   - Click alert in Security tab
   - Review code location and description
   - Check severity and confidence

2. **Fix Vulnerability**
   - Follow remediation guidance
   - Create fix in code
   - Submit PR with fix

3. **Dismiss Alert** (if false positive)
   - Click "Dismiss alert"
   - Select reason
   - Add comment explaining dismissal

### Dependabot Alerts

1. **Review Alert**
   - View CVE details and severity
   - Check affected dependency versions
   - Review patch availability

2. **Update Dependency**
   - Dependabot may auto-create PR
   - Review and merge PR
   - Or manually update dependency

3. **Snooze/Dismiss Alert**
   - If update not available
   - If vulnerability doesn't affect usage
   - Add reason and timeline

## Best Practices

1. **Regular Monitoring**
   - Review security alerts weekly
   - Prioritize critical and high severity issues
   - Keep dependencies up-to-date

2. **Pull Request Reviews**
   - Always review Dependabot PRs before merging
   - Run tests on dependency updates
   - Check for breaking changes

3. **Alert Triage**
   - Fix critical vulnerabilities immediately
   - Schedule fixes for high/medium issues
   - Evaluate false positives carefully

4. **Security Policy**
   - Maintain SECURITY.md for vulnerability reporting
   - Document security update process
   - Establish SLA for security patches

5. **Team Communication**
   - Assign security alerts to team members
   - Use GitHub notifications for alerts
   - Discuss security findings in stand-ups

## Customization

### Adjusting CodeQL Queries

Edit `.github/codeql/codeql-config.yml` to:
- Add custom queries
- Exclude specific queries
- Change query suites
- Adjust paths to scan

### Modifying Dependabot Behavior

Edit `.github/dependabot.yml` to:
- Change update frequency
- Adjust grouping strategy
- Add/remove ignored dependencies
- Configure auto-merge rules

### Security Scan Customization

Edit `.github/workflows/security-scan.yml` to:
- Add additional security tools
- Modify scan frequency
- Adjust scan parameters

## Troubleshooting

### CodeQL Analysis Fails

**Issue:** Build fails during CodeQL analysis

**Solutions:**
1. Check build logs in Actions tab
2. Ensure all dependencies are restored
3. Verify .NET SDK version matches
4. Check for build errors unrelated to CodeQL

### Dependabot PRs Not Created

**Issue:** No automated PRs from Dependabot

**Solutions:**
1. Verify `dependabot.yml` syntax
2. Check if updates are being ignored
3. Ensure open-pull-requests-limit not reached
4. Review Dependabot logs in Insights tab

### High Volume of Alerts

**Issue:** Too many security alerts

**Solutions:**
1. Prioritize by severity (critical/high first)
2. Group related issues and fix together
3. Update dependencies to latest stable versions
4. Review and dismiss false positives

## Additional Resources

- [GitHub Advanced Security Documentation](https://docs.github.com/en/code-security)
- [CodeQL Documentation](https://codeql.github.com/docs/)
- [Dependabot Documentation](https://docs.github.com/en/code-security/dependabot)
- [Security Best Practices](https://docs.github.com/en/code-security/getting-started/securing-your-organization)
- [CodeQL Query Reference](https://codeql.github.com/codeql-query-help/csharp/)

## Support

For questions or issues with GHAS:
1. Check GitHub documentation
2. Review workflow logs in Actions tab
3. Consult Security tab for specific alerts
4. Contact repository maintainers

## Version History

- **2025-10-25**: Initial GHAS setup
  - CodeQL for C# analysis
  - Dependabot for NuGet and GitHub Actions
  - Custom CodeQL configuration
  - Security scanning workflows
