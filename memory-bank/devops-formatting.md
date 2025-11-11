# Azure DevOps Markdown Formatting Guide

## Purpose

This document defines markdown formatting conventions for Azure DevOps work items to ensure compatibility and proper rendering when creating or updating work item descriptions.

## Why This Matters

Azure DevOps has specific markdown preferences when converting between HTML and markdown. Following these conventions ensures that content renders correctly without manual reformatting.

## Formatting Rules

### 1. Headers

**Use Setext-style for h2 headers:**
```markdown
Main Section
------------
```

**Use ATX-style for h3 and below:**
```markdown
### Subsection

#### Sub-subsection
```

**Why**: Azure DevOps converts h2 headers to Setext style (underlined with hyphens) during HTML-to-markdown conversion.

---

### 2. Code Blocks

**Use indented code blocks (4 spaces):**
```markdown
    public class Calculator
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
```

**DO NOT use fenced code blocks:**
```markdown
❌ Don't do this:
```csharp
public class Calculator { }
```
```

**Why**: Azure DevOps strips language hints during conversion and prefers indented code blocks. Language-specific syntax highlighting is lost during the conversion process.

---

### 3. Lists

**Use asterisks (`*`) for unordered lists:**
```markdown
*   First item
*   Second item
*   Third item
```

**Use proper indentation for nested lists (4 spaces):**
```markdown
*   Parent item
    *   Child item
    *   Another child
*   Another parent
```

**Use standard numbering for ordered lists:**
```markdown
1.  First step
2.  Second step
3.  Third step
```

**Why**: Azure DevOps converts hyphens to asterisks during HTML-to-markdown conversion.

---

### 4. Code Blocks Within Lists

**Indent code blocks within lists (8 spaces total):**
```markdown
*   Item with code example:
    
        var calculator = new Calculator();
        var result = calculator.Add(2, 3);
```

**Why**: Extra indentation maintains the list context while preserving code formatting.

---

### 5. Inline Code

**Use backticks as normal:**
```markdown
The `IContextualTestCaseBuilder` interface provides the fluent API.
```

**This remains unchanged across conversions.**

---

### 6. Bold and Emphasis

**Use standard markdown:**
```markdown
**Bold text**
*Italic text*
```

**This remains unchanged across conversions.**

---

### 7. Special Characters

**Use HTML entities for special characters in code:**
```markdown
*   `List&lt;string&gt;` for generic types
*   `Action&lt;T&gt;` for generic actions
*   Use `&amp;` for ampersands
```

**Why**: Ensures proper rendering of characters that could be interpreted as HTML/markdown syntax.

---

## Complete Example

Here's a complete example following these conventions:

```markdown
Overview
--------

Create a new component that provides a fluent API for writing behavior-driven tests.

Goals
-----

1.  Provide a fluent, hierarchical API
2.  Integrate with Microsoft.TestPlatform
3.  Support async/await patterns

Technical Architecture
----------------------

### Component Structure

    src/
    ├── Specifications.cs
    ├── Attributes/
    └── Discovery/

### Key Features

*   Custom test discoverer
*   Lifecycle hook support
*   Async/await support

### Code Example

    public class CalculatorSpecs : Specifications
    {
        [Context]
        public void calculator_specifications()
        {
            Calculator subject = null;
            int result = 0;
            
            Describe["Calculator"] = () =&gt;
            {
                BeforeEach = () =&gt; subject = new Calculator();
                
                It["should work"] = () =&gt; 
                    result.Should().Be(5);
            };
        }
    }

Dependencies
------------

*   Microsoft.TestPlatform.ObjectModel (&gt;= 17.0.0)
*   specs.core (internal dependency)
*   .NET Standard 2.1 / .NET 6+ support
```

---

## Quick Reference

| Element | Format | Example |
|---------|--------|---------|
| H2 Header | Underline with hyphens | `Section\n--------` |
| H3 Header | ATX style | `### Subsection` |
| Code Block | Indent 4 spaces | `    code here` |
| Unordered List | Asterisk + 3 spaces | `*   Item` |
| Ordered List | Number + period + 2 spaces | `1.  Item` |
| Inline Code | Backticks | `` `code` `` |
| Bold | Double asterisks | `**bold**` |

---

## Validation

Before updating Azure DevOps work items, verify:

- [ ] H2 headers use Setext style (underlined)
- [ ] H3+ headers use ATX style (`###`)
- [ ] Code blocks are indented (4 spaces)
- [ ] Lists use asterisks (`*`)
- [ ] Special characters use HTML entities
- [ ] No fenced code blocks with language hints

---

## Related Documentation

- [Azure DevOps Markdown Syntax](https://docs.microsoft.com/en-us/azure/devops/project/wiki/markdown-guidance)
- [memory-bank/README.md](README.md) - Memory bank overview

---

**Last Updated**: 2025-11-07
