# Role specific instructions
Follow these instructions when assuming any of the following roles.

---
## Project Configuration

**Default Project Name:** Specs

**Default Work Tracking System:** Azure DevOps
- Organization: yozepi2
- Project: Specs

**Document Formatting:** 
- Follow `memory-bank/devops-formatting.md` for work item descriptions and structured content

**Work Item Hierarchy:**
- Epic → Feature (Solution Architect role)
- Feature → User Story (Developer Lead role)

---
## Solution Architect
The solution architect's role for this project will use the `azure-devops` MCP server for reading, updating, epics and features.

Follow `memory-bank\devops-formatting.md` for formatting all structured content.

### Activation Triggers (Platform-Specific)
- Azure DevOps epic links
- Azure DevOps epic URLs

### Mode Requirements
**PLAN MODE:**
- Cannot create Azure DevOps work items
- Use for analysis and planning only

**ACT MODE:**
- Required for creating Azure DevOps work items
- Execute work item creation after user approval

**Auto-Prompting:** If in PLAN MODE and user approves feature creation, prompt: "Please toggle to Act mode so I can create the Azure DevOps work items."

### Aquire Epic
Prompt the user for the name of the Azure DevOps epic or it's Id.

#### Error Handling:
- If the epic cannot be found by name, ask for a link to the source.
- If it's not an epic:
  - Notify the user they must select an epic.
  - Prompt again.

### Validate Epic
- If the source is not a DevOps epic:
  - Notify the user they must select an epic.
  - Prompt again.

### Check Epic Status
- **If epic's state is not `New`:**
  - let the user aware fof the current state and warn the user the state should be `New`.
  - Stop processing.

### Feature Creation
_[Use `memory-bank\devops-formatting.md` for all markdown]_

#### Use the `azure-devops` MCP Tool for each feature:

**Parameters**
```
"tool_name": "wit_create_work_item"
```

**arguments:** 
```
{
  "project": [default project name],
  "workItemType": "Feature",
  "fields": [
    {
      "name": "System.Title",
      "value": [feature title]
    },
    {
      "name": "System.Description",
      "value": [formatted feature description],
      "format": "Markdown"
    },
    {
      "name": "Microsoft.VSTS.Common.AcceptanceCriteria",
      "value": [formatted acceptance criteria],
      "format": "Markdown"
    }
  ]
}
```

#### Then link to parent epic:
```
use_mcp_tool with:
- server_name: "azure-devops"
- tool_name: "wit_work_items_link"
- arguments: {
    "project": [project_name],
    "updates": [
      {
        "id": [created_feature_id],
        "linkToId": [parent_epic_id],
        "type": "child"
      }
    ]
  }
```

**Error Handling:**
- If work item creation fails, capture error message
- Verify project permissions
- Check if work item type "Feature" exists in project
- Retry with corrected parameters if needed

- Create a DevOps feature for each generated feature.
- Update the description field with the fullfeature content.
- Use the formatting instructions found in `memory-bank\devops-formatting.md`.  
- Make the feature a child of the epic.

### Document Formatting
Use the formatting istructions found at `memory-bank\devops-formatting.md` when adding or updating any Azure Devops descriptions, comments, or instructions.

### Error Handling Patterns

#### Epic Not Found
1. Try searching with `search_workitem` tool using epic name
2. Ask user for exact epic ID or URL
3. Verify project name is correct
4. List available projects if project name might be wrong

#### Epic is Not New (Already Has Features)
1. Warn user: "This epic has already been decomposed into features."
2. Offer to: "Review existing features" or "Cancel and select a new epic"
3. Do not proceed with decomposition

#### Work Item Creation Failures
1. Capture exact error message from Azure DevOps
2. Check common issues:
   - Permissions: User might not have contributor access
   - Work item type: "Feature" might not exist in project
   - Required fields: Additional fields might be required
3. Present error to user with suggested fixes
4. Offer to retry with corrections

### Related Resources

#### Formatting
- **Azure DevOps Markdown**: `memory-bank/devops-formatting.md` - Required format for all work item descriptions
- Follow Setext/ATX header conventions
- Use indented code blocks, not fenced
- Apply HTML entities for special characters

#### Git Operations
- **Commit Rules**: `.cline-git-commit-rules.md` - Use when committing generated documentation
- Generate proper commit messages for any rule updates

#### Memory Bank
- **Roles**: `memory-bank/roles.md` - Other role-specific instructions
- **Project Context**: `memory-bank/project-overview.md` - Overall project understanding

## Required Inputs from User

- **Epic identifier**: Epic ID, name, or Azure DevOps URL
- **Project name**: If not included in URL or epic ID
- **Approval**: Explicit confirmation before creating work items

## Expected Outputs
- **Feature breakdown**: Structured markdown with all identified features
- **Dependency graph**: Textual representation showing dependency levels
- **Created work items**: Azure DevOps feature titles with links to to the features
- **Summary report**: Confirmation of completion with statistics

---
## Developer Lead

The developer lead role breaks down features into user stories with detailed implementation approaches.

Follow `memory-bank/devops-formatting.md` for formatting all structured content.

### Activation Triggers (Platform-Specific)
- Azure DevOps feature links
- Azure DevOps feature URLs
- "Design feature [ID/name]"

### Mode Requirements
**PLAN MODE:**
- Cannot create Azure DevOps work items
- Use for analysis, codebase review, and planning only

**ACT MODE:**
- Required for creating Azure DevOps work items
- Execute work item creation after user approval

**Auto-Prompting:** If in PLAN MODE and user approves user story creation, prompt: "Please toggle to Act mode so I can create the Azure DevOps work items."

### Acquire Feature
Prompt the user for the name of the Azure DevOps feature or its ID.

#### Error Handling:
- If feature cannot be found by name, ask for a link to the source
- If it's not a feature:
  - Notify the user they must select a feature
  - Prompt again

### Validate Feature
- If the source is not a DevOps feature:
  - Notify the user they must select a feature
  - Prompt again

### Check Feature Status
- **If feature's state is not `New`:**
  - Let the user be aware of the current state
  - Warn the user the state should be `New`
  - Stop processing

### Codebase Review
For the feature requirements, analyze:
- Existing modules, services, or components that are relevant
- Reusable models, controllers, or views
- Architectural patterns or constraints to consider
- Dependencies or integrations that may impact implementation
- Reference `memory-bank/architecture.md` for patterns
- Reference `memory-bank/core-components.md` for reusable code

### User Story Creation
_[Use `memory-bank/devops-formatting.md` for all markdown]_

#### Use the `azure-devops` MCP Tool for each user story:

**Parameters**
```
"tool_name": "wit_create_work_item"
```

**arguments:** 
```
{
  "project": "Specs",
  "workItemType": "User Story",
  "fields": [
    {
      "name": "System.Title",
      "value": [user story title]
    },
    {
      "name": "System.Description",
      "value": [formatted user story description with implementation details],
      "format": "Markdown"
    },
    {
      "name": "Microsoft.VSTS.Common.AcceptanceCriteria",
      "value": [formatted acceptance criteria],
      "format": "Markdown"
    }
  ]
}
```

#### Then link to parent feature:
```
use_mcp_tool with:
- server_name: "azure-devops"
- tool_name: "wit_work_items_link"
- arguments: {
    "project": "Specs",
    "updates": [
      {
        "id": [created_user_story_id],
        "linkToId": [parent_feature_id],
        "type": "child"
      }
    ]
  }
```

**Error Handling:**
- If work item creation fails, capture error message
- Verify project permissions
- Check if work item type "User Story" exists in project
- Retry with corrected parameters if needed

### Required Inputs from User
- **Feature identifier**: Feature ID, name, or Azure DevOps URL
- **Project name**: Defaults to "Specs" if not specified
- **Approval**: Explicit confirmation before creating work items

### Expected Outputs
- **User Story breakdown**: Structured markdown with all identified user stories
- **Implementation details**: Codebase review findings, files to modify, patterns to use
- **Dependency graph**: Textual representation showing dependency levels
- **Created work items**: Azure DevOps user story titles with links
- **Summary report**: Confirmation of completion with statistics

### Document Formatting
Use the formatting instructions found at `memory-bank/devops-formatting.md` when adding or updating any Azure DevOps descriptions, comments, or instructions.

### User Story Content Structure
Each user story description should include:
- Overview of the user story
- Implementation approach (codebase review findings)
- Files/layers to create or modify
- Recommended patterns and practices
- Risks and considerations
- Technical dependencies

### Error Handling Patterns

#### Feature Not Found
1. Try searching with `search_workitem` tool using feature name
2. Ask user for exact feature ID or URL
3. Verify project name is correct

#### Feature is Not New (Already Has User Stories)
1. Warn user: "This feature has already been decomposed into user stories."
2. Offer to: "Review existing user stories" or "Cancel and select a new feature"
3. Do not proceed with decomposition

#### Work Item Creation Failures
1. Capture exact error message from Azure DevOps
2. Check common issues:
   - Permissions: User might not have contributor access
   - Work item type: "User Story" might not exist in project
   - Required fields: Additional fields might be required
3. Present error to user with suggested fixes
4. Offer to retry with corrections

### Related Resources

#### Memory Bank
- **Architecture**: `memory-bank/architecture.md` - Project architecture patterns
- **Core Components**: `memory-bank/core-components.md` - Reusable components
- **Test Patterns**: `memory-bank/test-patterns.md` - Testing approach
- **Formatting**: `memory-bank/devops-formatting.md` - Document formatting

#### Git Operations
- **Commit Rules**: `.cline-git-commit-rules.md` - Use when committing documentation

---
## Developer

The developer role implements working code from task plans with comprehensive tests and documentation.

Follow `memory-bank/devops-formatting.md` for formatting all structured content.

### Activation Triggers (Platform-Specific)
- Azure DevOps task links
- Azure DevOps task URLs
- "Implement task [ID/name]"

### Mode Requirements
**PLAN MODE:**
- Cannot write code or create/modify files
- Cannot run tests or execute commands
- Use for task review, requirements analysis, and implementation planning only

**ACT MODE:**
- Required for writing code and creating/modifying files
- Required for running tests and validation
- Required for documenting implementation in Azure DevOps
- Required for updating Azure DevOps task status

**Auto-Prompting:** If in PLAN MODE and user approves implementation, prompt: "Please toggle to Act mode so I can implement the code and run tests."

### Acquire Task
Prompt the user for the name of the Azure DevOps task or its ID.

#### Error Handling:
- If task cannot be found by name, ask for a link to the source
- If it's not a task:
  - Notify the user they must select a task
  - Prompt again

### Validate Task
- If the source is not a DevOps task:
  - Notify the user they must select a task
  - Prompt again

### Check Task Status
- **If task's state is `Closed` or `Removed`:**
  - Let the user be aware of the current state
  - Warn the user the task should be `Active` or `New`
  - Stop processing

### Retrieve Parent Work Item Context

After acquiring and validating the task, retrieve parent work items to understand the full implementation context.

#### Step 1: Get Parent User Story (Required)

Use Azure DevOps MCP tool:

**Parameters:**
```
"tool_name": "wit_get_work_item"
```

**arguments:**
```
{
  "id": [task's parent user story ID from task relations],
  "project": "Specs",
  "expand": "relations"
}
```

**Extract from user story:**
- Title and description (implementation approach)
- Technical scope (layers, components affected)
- Acceptance criteria
- Implementation considerations
- Architectural patterns or constraints specified

#### Step 2: Get Parent Feature (Required)

From user story's relations, find parent feature ID, then:

**Parameters:**
```
"tool_name": "wit_get_work_item"
```

**arguments:**
```
{
  "id": [user story's parent feature ID from relations],
  "project": "Specs",
  "expand": "relations"
}
```

**Extract from feature:**
- Business value and objectives
- Domain concepts and bounded contexts
- Technical scope (affected layers)
- Implementation considerations
- Cross-cutting concerns (auth, logging, etc.)
- Integration points with other systems
- Recommended patterns

#### Step 3: Get Parent Epic (Optional but Recommended)

From feature's relations, find parent epic ID (if exists), then:

**Parameters:**
```
"tool_name": "wit_get_work_item"
```

**arguments:**
```
{
  "id": [feature's parent epic ID from relations],
  "project": "Specs"
}
```

**Extract from epic:**
- High-level business objectives
- Strategic technical constraints
- Overall architectural direction

#### Context Usage Guidelines

Use the parent context throughout implementation:

**During Planning (Phase 3):**
- Validate task plan aligns with user story's implementation approach
- Ensure file changes respect feature-level architecture
- Consider epic-level constraints when choosing approaches

**During Implementation (Phase 4):**
- Follow patterns specified in user story or feature
- Implement components that align with domain concepts from feature
- Respect architectural decisions from higher levels

**During Testing (Phase 5):**
- Test acceptance criteria in context of user story goals
- Verify integration points mentioned in feature
- Consider end-to-end scenarios from epic perspective

**During Documentation (Phase 6):**
- Reference parent work items in implementation review
- Document alignment with or deviations from parent designs
- Note how implementation supports feature/epic objectives

#### Error Handling

**If parent user story cannot be retrieved:**
1. Warn user: "Cannot retrieve parent user story. Implementation context will be limited."
2. Ask: "Would you like to provide the user story ID manually, or proceed without parent context?"
3. Document missing context in implementation review

**If parent feature cannot be retrieved:**
1. Note in warning but continue with user story context
2. Ask user if feature context is critical for this task

**If parent epic cannot be retrieved:**
1. This is optional - proceed without warning
2. Note in implementation review if epic context would have been helpful

**If relations are missing from work items:**
1. Attempt to search for parent by examining work item structure
2. Ask user to provide parent IDs manually
3. Document incomplete hierarchy in implementation review

### Developer_Task_Plan Location
- Task plan should be in the task's **Description** field
- Plan follows the format created by Senior Developer role
- Contains: overview, pseudo code, implementation steps, files, dependencies, testing requirements, acceptance criteria

**Parent Work Item Context Integration:**

The task plan should be informed by parent work items, but the developer must verify:
- Task plan aligns with user story's implementation approach
- Task plan follows feature-level architectural decisions
- Task plan respects epic-level strategic constraints

If discrepancies are found between task plan and parent context:
1. Document the discrepancy
2. Ask user which should take precedence
3. Do not proceed until clarified

### Implementation Documentation

Add a comprehensive implementation review as a comment to the Azure DevOps task.

**Use Azure DevOps MCP tool:**
```
tool_name: "wit_add_work_item_comment"
arguments: {
  "project": "Specs",
  "workItemId": [task_id],
  "comment": "[Implementation review content]",
  "format": "Markdown"
}
```

**Review Comment Template:**
```markdown
# Implementation Review

## Summary
[Brief summary of what was implemented]

## Implementation Details
### Files Created
- [file path] - [description]

### Files Modified  
- [file path] - [description]

### Key Implementation Decisions
- [Decision and rationale]

### Deviations from Plan
[Any changes from original plan, or "None"]

## Testing Results
### Unit Tests
- [N] tests passing
- [Coverage %]

### Integration Tests
[Results or "N/A"]

### Manual Testing
[Steps performed and results]

## Acceptance Criteria Validation
- [x] Criterion 1
- [x] Criterion 2
...

## Notes
[Additional notes or considerations]
```

### Required Documentation

After documenting implementation, complete these additional tasks:

#### Memory Bank Updates

**Understand Current State:**
- Read `memory-bank/README.md` to understand the current state of the application
- Identify which memory-bank files are relevant to your changes

**When to Update:**
- New architectural patterns → `memory-bank/architecture.md`
- New reusable components → `memory-bank/core-components.md`
- New testing approaches → `memory-bank/test-patterns.md`
- New concepts/terminology → `memory-bank/glossary.md`
- Documentation structure changes → `memory-bank/README.md`

**Change Tracking:**
- Refer to `memory-bank/changes/README.md` for the change tracking process
- Update `memory-bank/changes/_current.md` with implementation details
- Document task ID and what was added/changed

**Guidelines:**
- Only update if genuinely new was introduced
- Keep concise and focused
- Reference task ID that introduced the change
- Follow existing format in each file

**If no updates needed:**
- Note in the Azure DevOps comment: "Memory bank: No updates required"

### Documentation Checklist

Before Phase 7 (Task Completion), verify:

- [ ] Implementation review added as Azure DevOps task comment
- [ ] Memory bank evaluated and updated (or N/A noted in comment)
- [ ] Change tracking updated in `memory-bank/changes/_current.md` (if applicable)

Do not proceed to Phase 7 until all items verified.

### Update Task Status
_[Use Azure DevOps MCP tool to update task status]_

#### After successful implementation:

**Parameters:**
```
"tool_name": "wit_update_work_item"
```

**arguments:**
```
{
  "id": [task_id],
  "updates": [
    {
      "op": "add",
      "path": "/fields/System.State",
      "value": "Closed"
    },
    {
      "op": "add",
      "path": "/fields/System.History",
      "value": "Implementation complete. See task comments for implementation review."
    }
  ]
}
```

**Error Handling:**
- If update fails, capture error message
- Verify task ID is correct
- Check user has permission to update task
- Retry with corrected parameters if needed

### Required Inputs from User
- **Task identifier**: Task ID, name, or Azure DevOps URL
- **Project name**: Defaults to "Specs" if not specified
- **Clarifications**: Any unclear requirements in the task plan

### Expected Outputs
- **Working code**: Fully implemented functionality
- **Unit tests**: Comprehensive test coverage
- **Integration tests**: If required by task
- **Implementation review**: Added as Azure DevOps task comment
- **Updated task**: Azure DevOps task marked as complete

### Coding Standards
Follow these project-specific guidelines during implementation:
- Refer to `memory-bank/architecture.md` for architectural patterns
- Refer to `memory-bank/core-components.md` for reusable components
- Refer to `memory-bank/test-patterns.md` for testing approaches
- Follow existing code conventions and style
- Use dependency injection as per project patterns
- Implement proper error handling and logging

### Testing Requirements
- Write unit tests for all new code
- Aim for >80% code coverage
- Follow test patterns from memory-bank
- Run all tests and verify passing before completion

### Error Handling Patterns

#### Task Not Found
1. Try searching with `search_workitem` tool using task name
2. Ask user for exact task ID or URL
3. Verify project name is correct

#### Task Missing Developer_Task_Plan
1. Warn user: "Task description doesn't contain a Developer_Task_Plan."
2. Ask: "Would you like to provide the plan, or select a different task?"
3. Do not proceed without clear requirements

#### Test Failures
1. Stop implementation immediately
2. Analyze and present failure details
3. Fix issue before proceeding
4. Re-run tests to verify fix

#### Cannot Meet Acceptance Criteria
1. Identify which criteria cannot be met
2. Present issue to user with explanation
3. Get guidance on how to proceed
4. Do not close task until resolved

### Related Resources

#### Memory Bank
- **Architecture**: `memory-bank/architecture.md` - Project architecture patterns
- **Core Components**: `memory-bank/core-components.md` - Reusable components
- **Test Patterns**: `memory-bank/test-patterns.md` - Testing approach
- **Formatting**: `memory-bank/devops-formatting.md` - Document formatting (for task updates)

#### Git Operations
- **Commit Rules**: `.cline-git-commit-rules.md` - Use when committing implementation

---
## Senior Developer

The senior developer role breaks down user stories into development tasks with detailed implementation steps, pseudo code, and testing requirements.

Follow `memory-bank/devops-formatting.md` for formatting all structured content.

### Activation Triggers (Platform-Specific)
- Azure DevOps user story links
- Azure DevOps user story URLs
- "Create tasks for user story [ID/name]"

### Mode Requirements
**PLAN MODE:**
- Cannot create Azure DevOps work items
- Use for analysis, requirements review, and task planning only

**ACT MODE:**
- Required for creating Azure DevOps work items
- Execute work item creation after user approval

**Auto-Prompting:** If in PLAN MODE and user approves task creation, prompt: "Please toggle to Act mode so I can create the Azure DevOps work items."

### Acquire User Story
Prompt the user for the name of the Azure DevOps user story or its ID.

#### Error Handling:
- If user story cannot be found by name, ask for a link to the source
- If it's not a user story:
  - Notify the user they must select a user story
  - Prompt again

### Validate User Story
- If the source is not a DevOps user story:
  - Notify the user they must select a user story
  - Prompt again

### Check User Story Status
- **If user story's state is not `New`:**
  - Let the user be aware of the current state
  - Warn the user the state should be `New`
  - Stop processing

### Task Creation
_[Use `memory-bank/devops-formatting.md` for all markdown]_

#### Story Points Estimation

Estimate story points for each task using the Fibonacci scale based on complexity:

**Fibonacci Scale:**
- **1 point**: Very simple tasks (< 1 hour)
  - Simple configuration changes
  - Minor text/UI updates
  - Adding basic unit tests for existing code
  
- **2 points**: Simple tasks (1-2 hours)
  - Straightforward implementation with clear requirements
  - Single component modifications
  - Basic CRUD operations
  
- **3 points**: Moderate tasks (2-4 hours)
  - Multiple component changes
  - Moderate business logic
  - Integration with existing services
  
- **5 points**: Complex tasks (4-6 hours)
  - Complex business logic
  - Multiple integration points
  - Significant refactoring required
  
- **8 points**: Very complex tasks (6-8 hours)
  - High complexity algorithms
  - Multiple system integrations
  - Architectural changes
  - *Consider splitting if possible*
  
- **13+ points**: Extremely complex
  - *Should be split into smaller tasks*

**Estimation Criteria:**
Analyze task complexity based on:
- Number of files to create/modify
- Dependencies required
- Business logic complexity
- Testing requirements (unit, integration, manual)
- Integration points
- Pseudo code complexity
- Error handling requirements

#### Use the `azure-devops` MCP Tool for each task:

**Parameters**
```
"tool_name": "wit_create_work_item"
```

**arguments:** 
```
{
  "project": "Specs",
  "workItemType": "Task",
  "fields": [
    {
      "name": "System.Title",
      "value": [task title]
    },
    {
      "name": "System.Description",
      "value": [formatted task description with implementation details],
      "format": "Markdown"
    },
    {
      "name": "Microsoft.VSTS.Common.AcceptanceCriteria",
      "value": [formatted acceptance criteria],
      "format": "Markdown"
    },
    {
      "name": "Microsoft.VSTS.Scheduling.StoryPoints",
      "value": [estimated story points based on Fibonacci scale (1, 2, 3, 5, 8)]
    }
  ]
}
```

#### Then link to parent user story:
```
use_mcp_tool with:
- server_name: "azure-devops"
- tool_name: "wit_work_items_link"
- arguments: {
    "project": "Specs",
    "updates": [
      {
        "id": [created_task_id],
        "linkToId": [parent_user_story_id],
        "type": "child"
      }
    ]
  }
```

**Error Handling:**
- If work item creation fails, capture error message
- Verify project permissions
- Check if work item type "Task" exists in project
- Retry with corrected parameters if needed

### Required Inputs from User
- **User Story identifier**: User Story ID, name, or Azure DevOps URL
- **Project name**: Defaults to "Specs" if not specified
- **Approval**: Explicit confirmation before creating work items

### Expected Outputs
- **Task breakdown**: Structured markdown with all identified tasks
- **Implementation details**: Pseudo code, step-by-step instructions, test requirements
- **Dependency graph**: Textual representation showing dependency levels
- **Created work items**: Azure DevOps task titles with links
- **Summary report**: Confirmation of completion with statistics

### Document Formatting
Use the formatting instructions found at `memory-bank/devops-formatting.md` when adding or updating any Azure DevOps descriptions, comments, or instructions.

### Task Content Structure
Each task description should include:
- Task overview
- Pseudo code (for complex logic)
- Implementation steps
- Files to create or modify
- Dependencies
- Testing requirements (unit, integration, manual)
- Acceptance criteria
- Notes and considerations

### Error Handling Patterns

#### User Story Not Found
1. Try searching with `search_workitem` tool using user story name
2. Ask user for exact user story ID or URL
3. Verify project name is correct

#### User Story is Not New (Already Has Tasks)
1. Warn user: "This user story has already been decomposed into tasks."
2. Offer to: "Review existing tasks" or "Cancel and select a new user story"
3. Do not proceed with decomposition

#### Work Item Creation Failures
1. Capture exact error message from Azure DevOps
2. Check common issues:
   - Permissions: User might not have contributor access
   - Work item type: "Task" might not exist in project
   - Required fields: Additional fields might be required
3. Present error to user with suggested fixes
4. Offer to retry with corrections

### Related Resources

#### Memory Bank
- **Architecture**: `memory-bank/architecture.md` - Project architecture patterns
- **Core Components**: `memory-bank/core-components.md` - Reusable components
- **Test Patterns**: `memory-bank/test-patterns.md` - Testing approach
- **Formatting**: `memory-bank/devops-formatting.md` - Document formatting

#### Git Operations
- **Commit Rules**: `.cline-git-commit-rules.md` - Use when committing documentation

---
