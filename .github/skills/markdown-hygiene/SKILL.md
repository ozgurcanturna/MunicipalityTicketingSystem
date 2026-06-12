---
name: markdown-hygiene
user-invocable: false
description: "Use when writing or reviewing README, docs, or other markdown files to avoid lint warnings, broken links, and inconsistent formatting."
---

# Markdown Hygiene

Use this skill when creating or updating markdown files in this repository.

## Goal

Produce markdown that is clean, lint-friendly, and easy to maintain.

## Checklist

- Prefer a single top-level H1 per document.
- Keep heading levels in order; do not skip levels.
- Add a language tag to every fenced code block.
- Use `text` or `plaintext` for tree, output, or non-code examples.
- Keep blank lines around headings, lists, tables, and fenced blocks.
- Use consistent list indentation and avoid mixed tabs or extra spaces.
- Keep table rows aligned and every row with the same number of columns.
- Use absolute workspace links only when referencing files in responses.
- Prefer repository-relative links inside markdown files.
- Avoid trailing spaces and double spaces at the end of lines.
- Use Mermaid fences only when the diagram is valid and the block stays readable.

## Before Writing

1. Scan the target file for existing style patterns.
2. Match the surrounding markdown style instead of introducing a new one.
3. If adding examples, choose the simplest block type that communicates the point.

## Before Finishing

1. Re-read the edited markdown for unlabeled fences, broken tables, and heading order.
2. Check that every link target exists in the workspace.
3. Prefer concise prose over decorative markdown when both work.