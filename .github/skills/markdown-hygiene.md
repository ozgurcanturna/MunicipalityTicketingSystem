# Markdown Hygiene & Documentation Standards

> **Purpose**: Ensure consistent, high-quality documentation across the project  
> **Last Updated**: 18.06.2026  

---

## 📋 Markdown Linting Rules

### Table Formatting (MD060)

Tables must have spaces around pipes for readability:

```markdown
# ✅ CORRECT
| Column 1 | Column 2 |
|----------|----------|
| Data 1   | Data 2   |

# ❌ WRONG
|Column 1|Column 2|
|--------|--------|
|Data 1|Data 2|
```

**Rule**: Always include a space after `|` and before `|` in table rows.

---

### Headings (MD022)

Headings must be surrounded by blank lines:

```markdown
# ✅ CORRECT

## Section Title

Content here...

# ❌ WRONG
## Section Title
Content here...
```

**Rule**: Add 1 blank line before and after every heading.

---

### Lists (MD032)

Lists must be surrounded by blank lines:

```markdown
# ✅ CORRECT

- Item 1
- Item 2
- Item 3

More content...

# ❌ WRONG
- Item 1
- Item 2
- Item 3
More content...
```

**Rule**: Add 1 blank line before and after every list.

---

### Blank Lines (MD012)

No multiple consecutive blank lines:

```markdown
# ✅ CORRECT

Line 1

Line 2

# ❌ WRONG

Line 1


Line 2
```

**Rule**: Maximum 1 consecutive blank line.

---

### Fenced Code Blocks (MD040)

Code blocks must specify language:

```markdown
# ✅ CORRECT

```typescript
const x: number = 1;
```

## ❌ WRONG

```typescript
const x: number = 1;
```

```markdown

**Rule**: Always specify language after opening code fence (e.g., `typescript`, `json`, `bash`).

---

## 🎯 Documentation Standards

## File Naming

- Use descriptive, camelCase names for markdown files
- Follow pattern: `Step-XX-Description.md` for step documents
- Use `README.md` for root documentation

## Structure

Every markdown file should have:

1. **Title** (H1)
2. **Metadata block** (blockquote with project info)
3. **Table of Contents** (for long documents)
4. **Sections** (H2, H3, etc.)
5. **References** (at the end)

## Code Examples

```typescript
// ✅ Include type annotations
interface User {
  id: string;
  name: string;
}

// ❌ Avoid any types
const data: any = {};
```

## Tables

- Always use proper spacing around pipes
- Keep columns aligned for readability
- Include headers for every table
- Use consistent formatting

### Links

- Use relative links for internal docs
- Use absolute links for external resources
- Test all links before committing

---

## 🔧 Configuration

### VS Code Settings

```json
{
  "markdown.lint": {
    "MD013": false, // Line length
    "MD024": false, // Multiple headings
    "MD029": false, // Ordered list prefix
    "MD033": false, // Inline HTML
    "MD041": false  // First line heading
  },
  "markdownlint.config": {
    "MD060": true,  // Table spacing (enabled)
    "MD022": true,  // Headings spacing (enabled)
    "MD032": true,  // Lists spacing (enabled)
    "MD012": true,  // Multiple blank lines (enabled)
    "MD040": true   // Code block language (enabled)
  }
}
```

### Pre-commit Hooks

Run markdown linting before commit:

```bash
# Install markdownlint
npm install -g markdownlint-cli

# Lint all markdown files
markdownlint docs/**/*.md
```

---

## 📝 Common Mistakes & Fixes

### Mistake 1: Missing Table Spacing

```markdown
# ❌ WRONG
|Header 1|Header 2|
|--------|--------|
|Data    |Data    |

# ✅ FIX
| Header 1 | Header 2 |
|----------|----------|
| Data     | Data     |
```

### Mistake 2: Heading Without Blank Lines

```markdown
# ❌ WRONG
## Section
Content

# ✅ FIX

## Section

Content
```

## Mistake 3: Code Block Without Language

```markdown
# ❌ WRONG
```typescript
const x = 1;
```

## ✅ FIX

```typescript
const x = 1;
```

```markdown

## Mistake 4: Multiple Consecutive Blank Lines

```markdown
# ❌ WRONG
Line 1


Line 2

# ✅ FIX
Line 1

Line 2
```

---

## 🚀 Workflow

## Before Commit

1. Run markdownlint: `markdownlint docs/**/*.md`
2. Fix any reported issues
3. Verify all links work
4. Check code examples compile

## Documentation Checklist

- [ ] All tables have proper spacing
- [ ] All headings surrounded by blank lines
- [ ] All lists surrounded by blank lines
- [ ] All code blocks have language specified
- [ ] No multiple consecutive blank lines
- [ ] All links are valid
- [ ] Code examples are accurate
- [ ] Consistent formatting throughout

---

**Last Updated**: 18.06.2026  
**Author**: Özgür Can TURNA
