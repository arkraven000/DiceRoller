# Security Requirements Document
## Warhammer 40K Dice Calculator - Windows 11 Desktop Application

**Version:** 1.0
**Date:** 2025-10-24
**Classification:** Public
**Framework Compliance:** OWASP Secure Design Principles, NIST SP 800-218 SSDF

---

## 1. Executive Summary

This document defines the security requirements for the Warhammer 40K Dice Calculator, a Windows 11 desktop application built using WinUI 3 and .NET 8. The application is designed with security-first principles, following OWASP and NIST frameworks to ensure data protection, secure coding practices, and defense against common attack vectors.

**Risk Profile:** LOW
**Rationale:** Offline desktop application with no network connectivity, no PII collection, and local-only data storage.

---

## 2. Security Objectives

### 2.1 Confidentiality
- Protect user-created unit profiles and scenarios from unauthorized access
- Ensure encryption of sensitive application data at rest
- Prevent information disclosure through error messages or logs

### 2.2 Integrity
- Ensure dice calculation accuracy and prevent tampering
- Maintain data integrity in local storage
- Verify application binary authenticity through code signing

### 2.3 Availability
- Application must fail gracefully without data loss
- Implement proper error handling to prevent crashes
- Ensure local data can be backed up and restored

---

## 3. Compliance Framework Mapping

### 3.1 OWASP Secure Design Principles

| Principle | Implementation | Priority |
|-----------|---------------|----------|
| Minimize Attack Surface | Offline-only, no network exposure, minimal dependencies | CRITICAL |
| Establish Secure Defaults | Encrypted storage, restrictive file permissions, secure PRAGMA settings | CRITICAL |
| Principle of Least Privilege | Run as standard user, minimal Windows capabilities | HIGH |
| Defense in Depth | Multiple validation layers, MVVM separation, type safety | HIGH |
| Fail Securely | Graceful error handling, no sensitive data in exceptions | MEDIUM |
| Don't Trust Services | Input validation, parameterized queries | HIGH |
| Separation of Duties | MVVM architecture enforces separation | MEDIUM |
| Avoid Security by Obscurity | Open security design, documented practices | LOW |
| Keep Security Simple | Standard crypto APIs, minimal custom security code | HIGH |
| Fix Security Issues Correctly | Automated tests, regression prevention | MEDIUM |

### 3.2 NIST SP 800-218 SSDF Practices

| Practice | Task | Implementation |
|----------|------|----------------|
| **PO.1** | Define Security Requirements | This document |
| **PO.3** | Protect Software | SQLCipher encryption, DPAPI key management |
| **PO.4** | Define Security Roles | Code review requirements, security champion |
| **PS.1** | Protect from Attack Surface | Type-safe language, managed runtime, no network |
| **PS.2** | Architect Secure Software | MVVM pattern, layered validation |
| **PS.3** | Review Architecture | Security design review completed |
| **PW.1** | Design Software Securely | Threat model included (Section 6) |
| **PW.2** | Review Software Design | Pre-implementation security review |
| **PW.4** | Reuse Existing Software | .NET BCL, SQLCipher, reputable NuGet packages |
| **PW.7** | Review Human-Readable Code | Code review checklist (Appendix A) |
| **PW.8** | Test Executable Code | xUnit security tests, static analysis |
| **RV.1** | Identify Vulnerabilities | NuGet vulnerability scanning, Dependabot |
| **RV.2** | Assess and Prioritize | CVSS scoring, patch within 30 days |
| **RV.3** | Respond to Vulnerabilities | Patch process, security advisory monitoring |

---

## 4. Detailed Security Requirements

### 4.1 Authentication & Authorization

**REQ-AUTH-001:** Application SHALL run under the context of the current Windows user account.
**Priority:** CRITICAL
**OWASP:** Principle of Least Privilege
**NIST:** PS.1

**REQ-AUTH-002:** Application SHALL NOT require or implement custom authentication mechanisms.
**Priority:** HIGH
**Rationale:** Single-user desktop application leverages OS-level user management.

**REQ-AUTH-003:** Application SHALL respect Windows User Account Control (UAC) settings.
**Priority:** CRITICAL
**OWASP:** Principle of Least Privilege
**Test:** Application must run without requesting elevation.

---

### 4.2 Data Protection

**REQ-DATA-001:** SQLite database SHALL be encrypted using SQLCipher with AES-256 encryption.
**Priority:** CRITICAL
**OWASP:** Establish Secure Defaults
**NIST:** PO.3, PS.1
**Implementation:** Use SQLCipher NuGet package version 3.8.13.5+

**REQ-DATA-002:** Database encryption keys SHALL be managed using Windows Data Protection API (DPAPI).
**Priority:** CRITICAL
**OWASP:** Don't Trust Services, Keep Security Simple
**NIST:** PO.3
**Implementation:** Store encrypted key in Windows Credential Manager

**REQ-DATA-003:** Database file SHALL be stored in user's %APPDATA%\DiceRoller\ directory with NTFS permissions limited to current user.
**Priority:** HIGH
**OWASP:** Principle of Least Privilege
**Test:** Verify file permissions = Current User (Full Control), SYSTEM (Read)

**REQ-DATA-004:** Application SHALL NOT store sensitive data in plaintext log files.
**Priority:** HIGH
**OWASP:** Fail Securely
**Implementation:** Sanitize all logged data, no encryption keys or user data in logs

**REQ-DATA-005:** Temporary files SHALL be securely deleted when no longer needed.
**Priority:** MEDIUM
**Implementation:** Use secure delete or overwrite before deletion

---

### 4.3 Input Validation

**REQ-INPUT-001:** All user inputs SHALL be validated in the ViewModel layer before processing.
**Priority:** CRITICAL
**OWASP:** Don't Trust Services, Defense in Depth
**NIST:** PS.2

**REQ-INPUT-002:** Numeric inputs SHALL be validated for range and type to prevent integer overflow.
**Priority:** HIGH
**Test Cases:**
- Reject negative attack counts
- Reject values > Int32.MaxValue
- Reject non-numeric input

**REQ-INPUT-003:** String inputs SHALL be validated for maximum length and sanitized before storage.
**Priority:** MEDIUM
**Implementation:**
- Unit names: Max 100 characters
- Descriptions: Max 500 characters
- Sanitize HTML/script tags

**REQ-INPUT-004:** All SQL queries SHALL use parameterized statements (prepared statements).
**Priority:** CRITICAL
**OWASP:** Don't Trust Services
**NIST:** PW.8
**Test:** No string concatenation in SQL queries, static analysis enforcement

---

### 4.4 Cryptography

**REQ-CRYPTO-001:** Application SHALL use only FIPS 140-2 compliant cryptographic algorithms.
**Priority:** HIGH
**OWASP:** Keep Security Simple
**NIST:** PO.3
**Approved Algorithms:**
- AES-256 for symmetric encryption
- SHA-256 or SHA-3 for hashing
- RSA-2048 minimum for asymmetric operations

**REQ-CRYPTO-002:** Application SHALL NOT implement custom cryptographic algorithms.
**Priority:** CRITICAL
**OWASP:** Keep Security Simple
**Implementation:** Use .NET BCL crypto APIs only

**REQ-CRYPTO-003:** Random number generation for dice simulation SHALL use cryptographically secure RNG.
**Priority:** MEDIUM
**Implementation:** Use System.Security.Cryptography.RandomNumberGenerator for fair dice rolls

**REQ-CRYPTO-004:** Application SHALL NOT hardcode cryptographic keys or secrets in source code.
**Priority:** CRITICAL
**OWASP:** Avoid Security by Obscurity
**Test:** Static analysis scan for hardcoded secrets

---

### 4.5 Error Handling & Logging

**REQ-ERROR-001:** Application SHALL implement graceful error handling that prevents application crashes.
**Priority:** HIGH
**OWASP:** Fail Securely
**NIST:** PS.1

**REQ-ERROR-002:** Error messages SHALL NOT expose sensitive information (file paths, stack traces, database schema).
**Priority:** HIGH
**OWASP:** Fail Securely
**Implementation:** Generic error messages to user, detailed logs to file

**REQ-ERROR-003:** Application SHALL log security-relevant events.
**Priority:** MEDIUM
**Events to Log:**
- Application start/stop
- Database encryption/decryption operations
- Input validation failures
- Unhandled exceptions

**REQ-ERROR-004:** Log files SHALL be stored in %APPDATA%\DiceRoller\Logs\ with restrictive permissions.
**Priority:** MEDIUM
**Implementation:** Rotate logs daily, retain for 30 days, max 100MB total

---

### 4.6 Code Quality & Security Testing

**REQ-TEST-001:** Application SHALL have automated unit tests with minimum 80% code coverage.
**Priority:** HIGH
**OWASP:** Fix Security Issues Correctly
**NIST:** PW.8

**REQ-TEST-002:** Security-critical functions SHALL have dedicated security tests.
**Priority:** CRITICAL
**NIST:** PW.8
**Test Categories:**
- Input validation boundary tests
- SQL injection attempt detection
- Cryptographic operation verification
- Integer overflow prevention
- File permission validation

**REQ-TEST-003:** Static code analysis SHALL be run on every build.
**Priority:** HIGH
**NIST:** PW.7
**Tools:**
- Roslyn analyzers
- Microsoft Security Code Analysis
- SonarQube (optional)

**REQ-TEST-004:** Third-party dependencies SHALL be scanned for known vulnerabilities.
**Priority:** CRITICAL
**NIST:** RV.1
**Implementation:** NuGet vulnerability scanning enabled in Visual Studio

---

### 4.7 Supply Chain Security

**REQ-SUPPLY-001:** All NuGet packages SHALL be from verified publishers or have minimum 1M downloads.
**Priority:** HIGH
**NIST:** PW.4, RV.1
**Exception Process:** Security review required for new packages

**REQ-SUPPLY-002:** NuGet package versions SHALL be pinned (not use floating versions).
**Priority:** MEDIUM
**Rationale:** Prevent automatic inclusion of compromised package versions

**REQ-SUPPLY-003:** Application binary SHALL be code-signed with a valid Authenticode certificate.
**Priority:** HIGH
**NIST:** PS.3
**Implementation:** Use EV code signing certificate

**REQ-SUPPLY-004:** Build process SHALL be reproducible and documented.
**Priority:** MEDIUM
**NIST:** PO.3

---

### 4.8 Deployment & Updates

**REQ-DEPLOY-001:** Application installer SHALL verify system requirements before installation.
**Priority:** MEDIUM
**Requirements:**
- Windows 11 22H2 or later
- .NET 8 Runtime or SDK

**REQ-DEPLOY-002:** Application SHALL install to user directory, not Program Files (avoiding elevation).
**Priority:** HIGH
**OWASP:** Principle of Least Privilege
**Location:** %LOCALAPPDATA%\Programs\DiceRoller\

**REQ-DEPLOY-003:** Application SHALL check for updates without exposing user data.
**Priority:** LOW (if implemented)
**Implementation:** Anonymous version check only, no telemetry

**REQ-DEPLOY-004:** Updates SHALL be delivered over HTTPS and signature-verified before installation.
**Priority:** CRITICAL (if implemented)
**NIST:** PS.3

---

### 4.9 Privacy & Data Collection

**REQ-PRIVACY-001:** Application SHALL NOT collect telemetry or usage data.
**Priority:** CRITICAL
**Rationale:** Offline-only application, no network connectivity

**REQ-PRIVACY-002:** Application SHALL NOT require internet connectivity for any core functionality.
**Priority:** CRITICAL
**OWASP:** Minimize Attack Surface

**REQ-PRIVACY-003:** Application SHALL NOT access user data outside its designated directories.
**Priority:** HIGH
**Allowed Directories:**
- %APPDATA%\DiceRoller\
- %LOCALAPPDATA%\DiceRoller\
- User-selected export locations (with explicit permission)

---

### 4.10 Windows Security Integration

**REQ-WIN-001:** Application SHALL declare minimum required Windows capabilities in manifest.
**Priority:** HIGH
**Required Capabilities:**
- File system access (user directories only)
- No network capabilities

**REQ-WIN-002:** Application SHALL be compatible with Windows Defender Application Control (WDAC).
**Priority:** MEDIUM
**Implementation:** Proper code signing enables WDAC policies

**REQ-WIN-003:** Application SHALL not disable or interfere with Windows security features.
**Priority:** CRITICAL
**Prohibited Actions:**
- Disabling Windows Defender
- Modifying firewall rules
- Disabling UAC

**REQ-WIN-004:** Application SHALL support Windows 11 security features.
**Priority:** MEDIUM
**Features:**
- Credential Guard compatibility
- Virtualization-Based Security (VBS) support
- Smart App Control compatibility (via code signing)

---

## 5. Security Architecture

### 5.1 Trust Boundaries

```
┌─────────────────────────────────────────────────────┐
│                    User (OS Level)                   │
│         Authenticated by Windows Logon               │
└────────────────┬────────────────────────────────────┘
                 │ Trust Boundary
                 ▼
┌─────────────────────────────────────────────────────┐
│              Application Process                     │
│  ┌──────────────────────────────────────────────┐  │
│  │              View (WinUI 3)                  │  │
│  │  - Display only, no business logic           │  │
│  │  - Input sanitization                        │  │
│  └────────────┬─────────────────────────────────┘  │
│               │                                      │
│               ▼                                      │
│  ┌──────────────────────────────────────────────┐  │
│  │           ViewModel (MVVM)                   │  │
│  │  - Input validation (PRIMARY)                │  │
│  │  - Business logic                            │  │
│  │  - Authorization checks                      │  │
│  └────────────┬─────────────────────────────────┘  │
│               │ Trust Boundary                       │
│               ▼                                      │
│  ┌──────────────────────────────────────────────┐  │
│  │             Model Layer                      │  │
│  │  - Data validation (SECONDARY)               │  │
│  │  - Database operations                       │  │
│  └────────────┬─────────────────────────────────┘  │
└───────────────┼──────────────────────────────────────┘
                │ Trust Boundary
                ▼
┌─────────────────────────────────────────────────────┐
│       SQLite Database (Encrypted at Rest)           │
│  - AES-256 encryption via SQLCipher                 │
│  - File system protection via NTFS ACLs             │
└─────────────────────────────────────────────────────┘
                │
                ▼
┌─────────────────────────────────────────────────────┐
│     Windows File System (NTFS)                      │
│  - OS-level access control                          │
└─────────────────────────────────────────────────────┘
```

### 5.2 Data Flow Security

**User Input → View:**
- Basic type validation (numeric, string length)
- UI-level format checking

**View → ViewModel:**
- Comprehensive input validation
- Range checking
- Business rule enforcement
- SQL injection prevention

**ViewModel → Model:**
- Data integrity checks
- Parameterized database queries
- Transaction management

**Model → Database:**
- Encrypted storage
- ACID compliance
- File system permissions

---

## 6. Threat Model

### 6.1 Assets

| Asset | Value | Confidentiality | Integrity | Availability |
|-------|-------|----------------|-----------|--------------|
| User's saved unit profiles | Medium | Medium | High | Medium |
| User's combat scenarios | Medium | Medium | High | Medium |
| Calculation history | Low | Low | Medium | Low |
| Application binary | High | N/A | Critical | High |
| Database encryption key | High | Critical | Critical | Medium |

### 6.2 Threat Actors

**Malicious Local User:**
- **Capability:** Physical access to machine, non-admin privileges
- **Motivation:** Data theft, application tampering
- **Mitigation:** File encryption, NTFS permissions, code signing

**Malware on System:**
- **Capability:** Code execution under user context
- **Motivation:** Data exfiltration, privilege escalation
- **Mitigation:** Minimal attack surface, no network exposure, secure coding

**Privileged Attacker (Admin):**
- **Capability:** Full system access
- **Motivation:** Complete compromise
- **Mitigation:** OUT OF SCOPE - OS-level security responsibility

### 6.3 Attack Scenarios & Mitigations

#### Scenario 1: SQL Injection Attack
**Attack Vector:** Malicious input in unit name field
**Example:** `"; DROP TABLE Units; --`
**Impact:** Data loss, application crash
**Likelihood:** Low (requires vulnerability in input handling)
**Mitigation:**
- REQ-INPUT-004: Parameterized queries only
- REQ-INPUT-003: Input sanitization
- REQ-TEST-002: Automated SQL injection tests
**Residual Risk:** MINIMAL

#### Scenario 2: Integer Overflow in Damage Calculation
**Attack Vector:** Extremely high attack counts
**Example:** Attacks=2,147,483,647, Damage=100
**Impact:** Incorrect calculations, application crash
**Likelihood:** Medium (user error or intentional abuse)
**Mitigation:**
- REQ-INPUT-002: Range validation
- REQ-TEST-002: Boundary value testing
- Calculation logic uses checked arithmetic
**Residual Risk:** LOW

#### Scenario 3: Database File Theft
**Attack Vector:** Physical theft of laptop or unauthorized file access
**Example:** Attacker copies database file to external device
**Impact:** Exposure of user's saved profiles
**Likelihood:** Medium
**Mitigation:**
- REQ-DATA-001: AES-256 database encryption
- REQ-DATA-002: DPAPI key management (key stays on original machine)
- REQ-DATA-003: NTFS permissions
**Residual Risk:** LOW (attacker cannot decrypt without DPAPI key)

#### Scenario 4: Malicious NuGet Package
**Attack Vector:** Compromised dependency in supply chain
**Example:** Popular package gets backdoored update
**Impact:** Code execution, data exfiltration
**Likelihood:** Low
**Mitigation:**
- REQ-SUPPLY-001: Verified publishers, high download counts
- REQ-SUPPLY-002: Pinned versions
- REQ-TEST-004: Vulnerability scanning
**Residual Risk:** LOW

#### Scenario 5: Path Traversal in Export Function
**Attack Vector:** Malicious path in export filename
**Example:** `../../../Windows/System32/malicious.dll`
**Impact:** Arbitrary file write, system compromise
**Likelihood:** Low
**Mitigation:**
- Use Windows file picker dialog (user explicit selection)
- Validate export paths
- Canonicalize paths before write
**Residual Risk:** MINIMAL

#### Scenario 6: Information Disclosure via Error Messages
**Attack Vector:** Triggering errors to expose system information
**Example:** Force database error to see file paths
**Impact:** Information leakage aids further attacks
**Likelihood:** Medium
**Mitigation:**
- REQ-ERROR-002: Generic user-facing error messages
- REQ-ERROR-004: Detailed errors only in logs (protected files)
**Residual Risk:** LOW

---

## 7. Security Controls Matrix

| Control Type | Control | Requirements | Priority |
|--------------|---------|--------------|----------|
| **Preventive** | Type Safety | REQ-INPUT-001, 002 | CRITICAL |
| **Preventive** | Input Validation | REQ-INPUT-001 to 004 | CRITICAL |
| **Preventive** | Parameterized Queries | REQ-INPUT-004 | CRITICAL |
| **Preventive** | Encryption at Rest | REQ-DATA-001, 002 | CRITICAL |
| **Preventive** | File Permissions | REQ-DATA-003 | HIGH |
| **Preventive** | Code Signing | REQ-SUPPLY-003 | HIGH |
| **Detective** | Vulnerability Scanning | REQ-TEST-004 | CRITICAL |
| **Detective** | Static Analysis | REQ-TEST-003 | HIGH |
| **Detective** | Security Logging | REQ-ERROR-003 | MEDIUM |
| **Corrective** | Error Handling | REQ-ERROR-001 | HIGH |
| **Corrective** | Automated Testing | REQ-TEST-001, 002 | HIGH |
| **Corrective** | Patch Management | REQ-SUPPLY-004, RV.3 | HIGH |

---

## 8. Security Development Lifecycle

### 8.1 Pre-Development
- [ ] Security requirements defined (this document)
- [ ] Threat model completed (Section 6)
- [ ] Security design review conducted
- [ ] Security champion assigned

### 8.2 During Development
- [ ] Secure coding guidelines followed
- [ ] Static analysis on every build
- [ ] Unit tests include security tests
- [ ] Code reviews include security checklist
- [ ] No hardcoded secrets (automated scan)

### 8.3 Pre-Release
- [ ] Security testing completed
- [ ] Penetration testing (if applicable)
- [ ] Dependency vulnerability scan passed
- [ ] Code signing certificate acquired
- [ ] Binary signed and verified

### 8.4 Post-Release
- [ ] Security advisory monitoring process
- [ ] Vulnerability disclosure process published
- [ ] Patch management process defined
- [ ] Incident response plan documented

---

## 9. Security Testing Requirements

### 9.1 Unit Tests (xUnit)

**Required Test Categories:**
1. **Input Validation Tests**
   - Boundary values (0, Int32.MaxValue, negative)
   - Invalid types (strings in numeric fields)
   - SQL injection patterns
   - XSS patterns (even though not web app, for future-proofing)

2. **Cryptographic Tests**
   - Encryption/decryption roundtrip
   - Key derivation correctness
   - Random number distribution (dice fairness)

3. **Data Integrity Tests**
   - Database ACID properties
   - Transaction rollback scenarios
   - Concurrent access handling

4. **Error Handling Tests**
   - Exception handling coverage
   - Graceful degradation
   - No sensitive data in error messages

### 9.2 Integration Tests

1. **Database Security Tests**
   - File encryption verification
   - Permission validation
   - Key management operations

2. **File System Tests**
   - NTFS permission verification
   - Secure file deletion
   - Path traversal prevention

### 9.3 Static Analysis Requirements

**Tools:**
- Microsoft Security Code Analysis (required)
- Roslyn analyzers (required)
- SonarQube Community Edition (optional)

**Scan Frequency:** Every build (CI/CD integration)

**Failure Criteria:**
- Any CRITICAL severity issue = build fails
- Any hardcoded secret detected = build fails
- SQL string concatenation detected = build fails

### 9.4 Dynamic Analysis

**Manual Security Testing:**
- [ ] Attempt SQL injection in all input fields
- [ ] Test with malicious filenames
- [ ] Verify file permissions post-installation
- [ ] Confirm database encryption
- [ ] Test integer overflow scenarios

---

## 10. Secure Configuration

### 10.1 SQLite Security Settings

**Required PRAGMA Settings:**
```sql
PRAGMA cipher_page_size = 4096;              -- Security: Standard page size
PRAGMA kdf_iter = 256000;                    -- Security: PBKDF2 iterations (high)
PRAGMA cipher_hmac_algorithm = HMAC_SHA512;  -- Security: Strong HMAC
PRAGMA cipher_kdf_algorithm = PBKDF2_HMAC_SHA512; -- Security: Strong KDF
PRAGMA foreign_keys = ON;                    -- Integrity: Enforce foreign keys
PRAGMA journal_mode = WAL;                   -- Reliability: Write-Ahead Logging
PRAGMA synchronous = FULL;                   -- Reliability: Ensure durability
```

### 10.2 .NET Security Settings

**Project Configuration:**
```xml
<PropertyGroup>
  <!-- Security: Treat warnings as errors -->
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>

  <!-- Security: Enable nullable reference types -->
  <Nullable>enable</Nullable>

  <!-- Security: Enable analyzers -->
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>

  <!-- Security: Code analysis -->
  <RunAnalyzersDuringBuild>true</RunAnalyzersDuringBuild>
  <RunAnalyzersDuringLiveAnalysis>true</RunAnalyzersDuringLiveAnalysis>
</PropertyGroup>
```

### 10.3 Windows Application Manifest

**Required Capabilities:**
```xml
<Capabilities>
  <!-- Minimal capabilities -->
  <rescap:Capability Name="runFullTrust" />
  <!-- NO network capabilities -->
  <!-- NO camera/microphone -->
  <!-- NO location services -->
</Capabilities>
```

---

## 11. Vulnerability Management

### 11.1 Vulnerability Identification

**Sources:**
- NuGet vulnerability scanning (automated)
- GitHub Dependabot alerts
- Microsoft Security Response Center (MSRC) advisories
- NIST National Vulnerability Database (NVD)

**Frequency:** Daily automated scans

### 11.2 Risk Assessment

**CVSS Scoring Guidelines:**
| CVSS Score | Severity | SLA for Patching |
|------------|----------|------------------|
| 9.0 - 10.0 | CRITICAL | 7 days |
| 7.0 - 8.9  | HIGH     | 30 days |
| 4.0 - 6.9  | MEDIUM   | 90 days |
| 0.1 - 3.9  | LOW      | Next release |

### 11.3 Patching Process

1. **Identification:** Automated scan identifies vulnerability
2. **Assessment:** Security team assesses applicability and risk
3. **Prioritization:** Assign priority based on CVSS and exploitability
4. **Remediation:** Update dependency or mitigate vulnerability
5. **Testing:** Run full test suite including security tests
6. **Deployment:** Release patch according to SLA
7. **Verification:** Confirm vulnerability no longer present

---

## 12. Incident Response

### 12.1 Security Incident Definition

**Examples:**
- Discovery of critical vulnerability in application
- Compromise of code signing certificate
- Malware detection in build environment
- Unauthorized access to source code repository
- User reports of data loss or corruption

### 12.2 Response Procedure

1. **Detection:** Incident identified through monitoring or report
2. **Containment:**
   - Halt releases if necessary
   - Revoke compromised certificates
   - Secure affected systems
3. **Investigation:**
   - Determine scope and impact
   - Identify root cause
   - Document findings
4. **Remediation:**
   - Develop and test fix
   - Prepare security advisory
   - Deploy patch
5. **Recovery:**
   - Verify fix effectiveness
   - Monitor for recurrence
6. **Post-Incident:**
   - Lessons learned review
   - Update security requirements
   - Improve detection mechanisms

---

## 13. Compliance Checklist

### 13.1 OWASP Secure Design Principles

- [x] Minimize Attack Surface Area
- [x] Establish Secure Defaults
- [x] Principle of Least Privilege
- [x] Defense in Depth
- [x] Fail Securely
- [x] Don't Trust Services
- [x] Separation of Duties
- [x] Avoid Security by Obscurity
- [x] Keep Security Simple
- [x] Fix Security Issues Correctly

### 13.2 NIST SSDF Core Practices

**Prepare the Organization (PO):**
- [x] PO.1: Define security requirements
- [x] PO.3: Protect software
- [x] PO.4: Define security roles
- [ ] PO.5: Implement security-supportive tools

**Protect the Software (PS):**
- [x] PS.1: Protect from attack surface reduction
- [x] PS.2: Architect software securely
- [x] PS.3: Verify third-party components

**Produce Well-Secured Software (PW):**
- [x] PW.1: Design software securely
- [x] PW.2: Review software design
- [x] PW.4: Reuse existing well-secured software
- [x] PW.7: Review code for security
- [x] PW.8: Test code for security

**Respond to Vulnerabilities (RV):**
- [x] RV.1: Identify vulnerabilities
- [x] RV.2: Assess and prioritize vulnerabilities
- [x] RV.3: Respond to vulnerabilities

---

## 14. Responsibilities

| Role | Security Responsibilities |
|------|---------------------------|
| **Security Champion** | - Conduct security design reviews<br>- Approve security-sensitive code changes<br>- Monitor security advisories<br>- Coordinate incident response |
| **Developers** | - Follow secure coding guidelines<br>- Write security unit tests<br>- Participate in code reviews<br>- Fix assigned vulnerabilities within SLA |
| **QA/Test** | - Execute security test plans<br>- Verify vulnerability fixes<br>- Perform regression testing<br>- Document security defects |
| **DevOps** | - Configure security scanning tools<br>- Manage code signing certificates<br>- Secure build environment<br>- Monitor automated security checks |

---

## 15. References

### 15.1 Standards & Frameworks

- **OWASP Top 10 2021:** https://owasp.org/www-project-top-ten/
- **OWASP Secure Coding Practices:** https://owasp.org/www-project-secure-coding-practices-quick-reference-guide/
- **NIST SP 800-218 SSDF:** https://csrc.nist.gov/publications/detail/sp/800-218/final
- **Microsoft Security Development Lifecycle:** https://www.microsoft.com/en-us/securityengineering/sdl

### 15.2 Technical Documentation

- **.NET Security:** https://learn.microsoft.com/en-us/dotnet/standard/security/
- **SQLCipher Documentation:** https://www.zetetic.net/sqlcipher/documentation/
- **Windows Data Protection API:** https://learn.microsoft.com/en-us/windows/win32/seccng/cng-dpapi
- **WinUI 3 Security:** https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/

---

## Appendix A: Secure Code Review Checklist

### Input Validation
- [ ] All user inputs validated at ViewModel layer
- [ ] Numeric inputs checked for overflow/underflow
- [ ] String inputs checked for length limits
- [ ] No direct user input in SQL queries
- [ ] File paths validated and canonicalized

### Cryptography
- [ ] No custom crypto implementations
- [ ] Only approved algorithms used (AES-256, SHA-256/SHA-3)
- [ ] No hardcoded keys or secrets
- [ ] Cryptographically secure RNG for dice rolls
- [ ] Proper key derivation (PBKDF2)

### Data Protection
- [ ] Sensitive data encrypted at rest
- [ ] Database uses parameterized queries only
- [ ] DPAPI used for key management
- [ ] File permissions set correctly
- [ ] No sensitive data in logs

### Error Handling
- [ ] Try-catch blocks around risky operations
- [ ] Generic error messages to users
- [ ] Detailed errors logged securely
- [ ] No unhandled exceptions
- [ ] Graceful degradation implemented

### Dependencies
- [ ] All packages from verified sources
- [ ] Package versions pinned
- [ ] No known vulnerabilities (scan passed)
- [ ] Minimal dependencies used
- [ ] Licenses reviewed

### Architecture
- [ ] MVVM separation maintained
- [ ] Trust boundaries enforced
- [ ] Validation in multiple layers
- [ ] Principle of least privilege applied
- [ ] No unnecessary capabilities requested

---

## Appendix B: Security Test Cases

### Test Case: SQL-001 - SQL Injection Prevention
**Objective:** Verify application rejects SQL injection attempts
**Input:** Unit name = `'; DROP TABLE Units; --`
**Expected:** Input sanitized or rejected, no database error
**Priority:** CRITICAL

### Test Case: CRYPTO-001 - Database Encryption
**Objective:** Verify database file is encrypted
**Steps:**
1. Create unit profile and save
2. Close application
3. Attempt to open database file with standard SQLite tool
**Expected:** Unable to read database without encryption key
**Priority:** CRITICAL

### Test Case: INPUT-001 - Integer Overflow
**Objective:** Verify integer overflow protection
**Input:** Attacks = Int32.MaxValue, Damage = 100
**Expected:** Validation error or safe calculation with appropriate limits
**Priority:** HIGH

### Test Case: AUTH-001 - File Permissions
**Objective:** Verify database file has correct permissions
**Steps:**
1. Install and run application
2. Check NTFS permissions on database file
**Expected:** Current user only (no Everyone, no Users group)
**Priority:** HIGH

### Test Case: ERROR-001 - Information Disclosure
**Objective:** Verify errors don't expose sensitive information
**Steps:**
1. Cause various error conditions
2. Examine error messages shown to user
**Expected:** No file paths, stack traces, or database schema exposed
**Priority:** MEDIUM

---

## Appendix C: Glossary

**AES-256:** Advanced Encryption Standard with 256-bit key, symmetric encryption algorithm
**CVSS:** Common Vulnerability Scoring System, standardized vulnerability severity rating
**DPAPI:** Data Protection API, Windows encryption service for protecting sensitive data
**FIPS 140-2:** Federal Information Processing Standard for cryptographic modules
**NTFS ACL:** New Technology File System Access Control List, Windows file permissions
**OWASP:** Open Web Application Security Project
**PBKDF2:** Password-Based Key Derivation Function 2, key stretching algorithm
**RNG:** Random Number Generator
**SQLCipher:** Open-source SQLite extension providing 256-bit AES encryption
**SSDF:** Secure Software Development Framework (NIST SP 800-218)
**UAC:** User Account Control, Windows privilege escalation security feature
**WDAC:** Windows Defender Application Control, code integrity policy enforcement

---

## Document Control

**Version History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-10-24 | Claude | Initial security requirements document |

**Review Schedule:** Quarterly or upon significant application changes

**Next Review Date:** 2026-01-24

**Document Owner:** Security Champion

**Approval:**
- [ ] Security Champion
- [ ] Lead Developer
- [ ] Project Manager

---

**END OF DOCUMENT**
