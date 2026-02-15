# Project Brief: Specs Testing Framework

## Project Identity

**Name**: Specs  
**Type**: .NET Testing Framework  
**Target**: .NET Standard 2.1  
**Repository**: https://github.com/yozepi/Specs

## Core Purpose

Specs is a .NET testing framework that provides TWO different BDD-style testing approaches for hierarchical test organization with sophisticated lifecycle management and full async/await support.

## Primary Goals

1. **Structured BDD Testing**: Provide intuitive APIs for behavior-driven development
2. **Hierarchical Organization**: Enable nested test contexts with inherited behavior
3. **Lifecycle Management**: Support comprehensive setup/teardown patterns (SetupOnce, SetupEach, Act, TeardownEach, TeardownOnce)
4. **Async Support**: Full async/await support across all lifecycle hooks
5. **Framework Flexibility**: Offer two distinct API styles to suit different preferences
6. **Framework Agnostic Core**: Maintain independence through adapter pattern

## Solution Structure

### Deliverable Projects (2 projects)
1. **specs.core** - Core testing framework library (deliverable)
2. **specs.BDD** (specs.specification) - High-level BDD API (deliverable)

### Test Projects (2 projects)
3. **specs.core.UnitTests** - Unit tests for the core framework
4. **specs.BDD.UnitTests** - Tests for BDD library

### Internal Testing Infrastructure (1 project)
5. **TestHarness** - Temporary NSpec integration for internal testing
   - Used only to test Specs framework itself during development
   - Not part of project output or deliverables
   - Will be phased out once Specs is mature enough to test itself

## Two Testing Approaches

### Modern Approach: specs.BDD (`Specifications` class)
- Method-based lifecycle API: `BeforeAll()`, `BeforeEach()`, `Act()`
- Builder properties: `Describe`, `Context`, `It`, `ItAsync`
- Built on immutable test case models
- Framework-agnostic through adapters

### Legacy Approach: `Spec` class
- Property-based lifecycle API: `BeforeEach = ...`, `Act = ...`
- Direct runtime integration
- Built-in exception testing: `ShouldThrow()` methods
- Mutable runtime context

## Target Audience

- .NET developers writing behavior-driven tests
- Teams requiring sophisticated test setup/teardown patterns
- Projects working with async/await patterns in tests
- Teams preferring either property-based or method-based test APIs

## Success Criteria

1. Enable hierarchical test organization through contexts
2. Provide clean, intuitive APIs for test construction
3. Support both synchronous and asynchronous operations
4. Maintain clear separation between test lifecycle phases
5. Remain framework-agnostic through adapter pattern
6. Support gradual migration from legacy to modern approach

## Technology Stack

- **Platform**: .NET Standard 2.1
- **Test Frameworks**: NSpec (BDD), NUnit (unit testing)
- **Assertions**: NFluent (fluent assertions)
- **Language**: C# with nullable reference types enabled

## Project Scope

**In Scope**:
- Hierarchical test case building
- Comprehensive lifecycle management
- Async/await support
- Two distinct API styles
- Framework integration through adapters
- NSpec integration

**Out of Scope**:
- Test runners (relies on NSpec/NUnit)
- Assertion libraries (uses NFluent)
- IDE integration
- Test discovery mechanisms

## Key Constraints

1. Must maintain backward compatibility with legacy `Spec` class
2. Must target .NET Standard 2.1 for broad compatibility
3. Must integrate with existing NSpec ecosystem
4. Core framework must remain framework-agnostic
5. Both API approaches must coexist

## Success Metrics

- Comprehensive test coverage (>80%)
- Clear, maintainable code
- Documented architecture and patterns
- Working examples for both APIs
- Smooth migration path between approaches