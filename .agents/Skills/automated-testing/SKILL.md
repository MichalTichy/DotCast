---
name: automated-testing
description: Perform exploratory testing of a web application with Playwright.
---

# Single-session exploratory testing with Playwright

You are the live browser exploratory test executor.
You playwright-cli for inspecting the aplication.
Do NOT generate Playwright spec files unless explicitly requested.

## Mission

Explore the application, perform all possible actions, validate expected behavior, find real defects, minimize false failures, and finish the intended coverage before stopping.

## Non-goals

- Do not stop while required coverage items remain untested.
- Do not fail with weak evidence when the right result is INCONCLUSIVE or BLOCKED.

## Allowed verdicts

Use only these verdicts:
- PASS
- FAIL
- INCONCLUSIVE
- BLOCKED

Definitions:
- PASS = expected behavior is positively supported by evidence.
- FAIL = expected behavior is contradicted by positive evidence.
- INCONCLUSIVE = evidence is insufficient, mixed, or the UI changed in a way that cannot yet be classified.
- BLOCKED = environment, access, or a hard app failure prevents meaningful continuation.

Do not convert INCONCLUSIVE into FAIL without stronger evidence.

## Required execution phases

### Phase 0 — Context gathering and complete checklist building

Determine the entry point/environment. Use the user-provided environment when spinning up the local environment (via Aspire, if available); otherwise, use the default one.

Before browser work, gather context and build complete checklist and coverage ledger template. Your goal is to capture every possible action, expected outcome, and evidence signal from all available sources to create a comprehensive exploratory checklist that will guide the testing session.

Compile checklist items from ALL of these sources - DO NOT SKIP ANY SOURCE, and be exhaustive in capturing every possible action and outcome.
Your goal is to create a checklist that is as complete as possible before starting the exploration, but be prepared to update it in real time as you discover new items during the run.

Sources to gather from:
1. Retrieve requirements and features from relevant sources (via MCP server ADO - check git settings to retrieve project information ) - there can be 100+ of each and you have to get them all - use information found in Features and Requirements to add flows that need to be covered in checklist.
2. Retrieve list of Usecases/Handlers (user actions - Classes with name containing "Usecase")- Add them as actions to be covered in the checklist, along with their expected outcomes and required roles/permissions.
3. Retrieve list of API endpoints. Retrieve swagger documentation is available at `/swagger` route in the app. - Add API operations as actions to be covered in the checklist, along with their expected outcomes and required roles/permissions.
4. Retrieve list of pages, dialogs and tabs - Add checking all of them to the checklist together, include all possible actions in the checklist.
5. Extract from the above sources a list of target features and flows to cover, along with their expected outcomes and required roles/permissions. Do happy path and edge case scenarios for each flow. Add them to the checklist.
6. Build a comprehensive checklist - include every planned action as separate checklist item - for actions that have happy path and edge case scenarios, create separate checklist items for each scenario. For any action that has multiple possible outcomes based on different inputs or states, create separate checklist items for each expected outcome. For any action that has different expected outcomes based on user roles or permissions, create separate checklist items for each role/permission combination. Each checklist item should include:
   - action
   - type of check (visual, happy path, edge case,...)
   - expected observable outcome
   - evidence signal to capture
   - parent flow or feature
   - branch or variant when applicable
8. Order checklist items by logical flow and dependencies, but be prepared to adapt as you explore. For example, if a certain page must be visited before a particular action can be performed, order the checklist accordingly. However, if during exploration you discover that an action can be performed from multiple entry points, be flexible in the order of testing those entry points.
9. Store the checklist and coverage ledger template for use during the run as a file.

You are expected to perform all possible actions (trigger all usecasses and API operations) with Happy path and edge case scenarios, and to cover all pages, dialogs, and tabs. Do not skip any of them.

Update the checklist and coverage ledger in real time during the run as you discover new actions, expected outcomes, or evidence signals that were not captured in the original checklist. Do not leave any discovered item unrecorded.

Only after you have build a comprehensive checklist and coverage ledger template, and you have stored them for use during the run, you can move to the next phase of execution.
If you discover during the run that your original checklist is missing items, update it immediately and do not leave any discovered item unrecorded. Every item in the checklist must be completed or classified as BLOCKED/INCONCLUSIVE before stopping.
### Phase 1 — Access and authentication

Treat login as a finite state machine, not an open-ended retry loop.

Before attempting login, define the login success oracle for this app.
Login success is established only when at least 2 of these are true:
- login form is absent or hidden
- protected route or known post-login URL is reached
- authenticated app chrome appears (logout button, user menu, avatar, protected nav)
- expected protected page heading or main container is visible
- a previously blocked feature is now accessible

Authentication order:
1. Reuse existing authenticated state if already present.
2. If provided, use storage/auth state once.
3. Try provided credentials once.
4. If that fails, look for `testingCredentials.txt` or another explicitly provided credential source once.
5. If all known auth methods fail, stop with BLOCKED.

Rules:
- Never submit the same credential set more than 2 times to the same form without new evidence.
- After each auth attempt, check the login success oracle.
- If auth fails, record the exact visible error, route, and form state.
- If auth succeeds, record the auth method used in the coverage ledger.

If the app appears stuck between login and post-login state:
- inspect URL
- inspect visible errors/toasts
- inspect whether a redirect loop is happening
- inspect whether a protected page partially loaded
Then classify as FAIL only with evidence; otherwise BLOCKED or INCONCLUSIVE.

### Phase 2 — Build and maintain the coverage ledger

Maintain a live COVERAGE LEDGER throughout the run.

Required fields:
- environment used
- auth method used
- current authenticated user/role if visible
- top-level navigation items discovered
- visited top-level sections
- visited routes
- visited secondary navigation (tabs, accordions, subtabs, menus)
- dialogs/overlays opened
- discovered during run checklist items
- state-changing flows completed
- destructive actions completed
- cleanup actions completed
- checklist items completed
- checklist items remaining
- checklist item statuses: TODO / IN_PROGRESS / PASS / FAIL / INCONCLUSIVE / BLOCKED
- screenshot artifact index
- visual inspection notes for high-impact actions
- anomalies observed
- blockers
- remaining queue

### Phase 3 — Traversal strategy

Use a hybrid traversal strategy:
- breadth-first to inventory the application structure
- depth-first to complete an active feature flow once entered

Start by inventorying all visible top-level entry points:
- sidebar
- top navigation
- hamburger menus
- user menus
- dashboards with cards that act as navigation

Put them into a queue.

Rules:
1. Visit each top-level section at least once unless blocked by role/access.
2. Inside a section, inventory secondary navigation and add it to a sub-queue.
3. If a target feature flow has started, finish that flow to a terminal state before leaving it:
   - saved
   - cancelled
   - closed
   - explicit validation result
   - reproduced bug
   - confirmed blocker
4. Do NOT leave a flow halfway through solely because of an arbitrary action limit.
5. Update checklist and coverage ledger in real time as you go.
6. Any newly discovered action, route, button, menu item, tab, dialog action, row action, or API-backed operation that was not in the original checklist must be appended to the checklist immediately with:
   - action
   - expected observable outcome
   - evidence signal
   - parent feature/flow
   - status = TODO
7. Newly discovered actions are first-class checklist items and must be completed before stopping unless explicitly classified as BLOCKED or INCONCLUSIVE.

Meaningful actions include:
- navigation
- opening or closing overlays
- selecting tabs, filters, accordions, menus, row actions
- filling and submitting forms
- saving, cancelling, deleting, confirming
- bulk actions
- export/import actions
- row-level actions in grids or lists
- any control that can trigger a distinct UI or server state change
- any action expected to produce observable evidence

### Phase 3.2 — Branch discovery and expansion

When the UI reveals multiple distinct user-selectable paths from the current flow, you must immediately expand them into separate checklist items before continuing.

A distinct path is any newly revealed option, action, state, or variant that can produce a meaningfully different outcome.

Common examples:
- multiple buttons in a dialog, drawer, or popup
- menu items in an action menu
- radio options or select options with different downstream behavior
- tabs, subtabs, accordions, or wizard steps
- row actions in tables or lists
- file formats, report types, delivery modes, or destinations
- filters or toggles that materially change loaded data or behavior
- confirm, cancel, retry, preview, or secondary-action branches
- role-based or state-based variants exposed only after interaction

Rules:
1. Do not keep newly revealed branches implicit inside one checklist item.
2. Add each distinct branch as its own checklist item as soon as it becomes visible.
3. Record the parent flow and the branch or variant for each newly created item.
4. Do not mark the parent item complete until all discovered branches are resolved as PASS, FAIL, INCONCLUSIVE, or BLOCKED.
5. If a branch is not executed, it must still be explicitly classified. It cannot remain hidden inside a completed parent item.
6. If two visible options are truly equivalent, record that equivalence explicitly in the checklist or ledger before treating them as one item.

### Phase 3.3 — Composite interaction completion gate

An interaction is not complete merely because its container opened.

If opening a page, dialog, menu, tab set, wizard, drawer, or panel reveals additional executable choices, the interaction is only complete after:
- the revealed choices have been inventoried
- each distinct choice has been added to the checklist
- each choice has been executed or explicitly classified

Opening something and exercising only one visible branch is insufficient coverage unless no other meaningful branches exist.

Before resolving any checklist item as PASS, explicitly check whether the current UI still exposes any unexecuted options, formats, actions, tabs, toggles, or states that belong to the same flow. If yes, append them first.

### Phase 3.5 — State-changing and destructive flows

State-changing actions are required coverage, not optional.

Definitions:
- non-destructive = open, close, navigate, filter, validate, cancel
- state-changing = create, edit, save, submit, approve, reject, archive, unarchive
- destructive = delete, remove, revoke, deactivate, unlink, reset, clear, irreversible confirm actions

Rules:
1. If a feature exposes a state-changing or destructive action, add it to the checklist and test it unless blocked by permissions, environment safety, or missing disposable data.
2. Prefer full lifecycle order when available:
   - create
   - verify persisted state
   - edit
   - verify persisted state
   - execute downstream actions on the edited entity
   - destructive action last
   - verify the entity is removed or final state is reflected
3. Never perform a destructive action before all meaningful non-destructive and state-changing actions for that entity/flow are completed.
4. Use disposable test data whenever possible.
5. If destructive cleanup is possible and safe, perform it before leaving the flow.
6. If destructive action is unsafe or irreversible for shared data, create disposable data first and destroy only that data.
7. If no safe disposable target can be created, classify the destructive step as BLOCKED or INCONCLUSIVE with reason; do not skip silently.

### Phase 3.6 — Screenshot and visual inspection protocol

Screenshots are mandatory evidence for high-impact actions and for every encountered problem.

High-impact actions include:
- opening or navigating to a new page or route
- login, logout, or auth-state transitions
- opening a business-relevant dialog, drawer, wizard, or detail page
- save, submit, create, edit, approve, reject, import, export, delete, or other state-changing actions
- major tab switches or filters that materially change loaded content
- any action whose success depends on the page looking correct, not only on DOM presence

Rules:
1. After every high-impact action, wait for the UI to stabilize, then capture a screenshot before moving on.
2. Immediately inspect the screenshot visually; do not treat screenshot capture as a passive artifact dump.
3. Visual inspection must check for obvious rendering or UX defects such as blank or half-rendered pages, stale spinners, missing assets, overlapped or clipped controls, broken spacing, unreadable text, duplicated overlays, unexpected error banners, and content rendered outside the viewport.
4. If the page contains dynamic regions, focus the inspection on the areas whose correctness matters for the current checklist item.
5. Record the screenshot path and a one-line visual verdict in the coverage ledger or checklist evidence field.
6. When any problem is encountered, capture a screenshot immediately before recovery or retry if possible.
7. If a retry, refresh, or alternate interaction path changes the UI state, capture a follow-up screenshot after that change as well.
8. Treat visual anomalies as first-class evidence. Even if the DOM looks plausible, a clearly broken screenshot can justify FAIL or INCONCLUSIVE depending on evidence strength.
9. Store screenshots under `AiTesting/{DATE}/{RUNID}/Artifacts/Screenshots/` using stable names that tie back to the checklist item or finding.

### Phase 4 — Action contract

For every meaningful action:
1. State the expected primary outcome before the action.
2. State 1–2 acceptable alternate signals.
3. Perform the action once.
4. Wait for UI-state signals, not fixed delays.
5. Verify whether the expected outcome occurred.
6. If the action is high-impact, capture and visually inspect a screenshot before resolving the step.
7. If any problem or ambiguity is observed, capture a problem screenshot immediately and preserve it before retrying.
8. If not, try up to 2 alternative interaction approaches.
9. After each state-changing action, verify that known error overlays are not visible.

Examples of acceptable state signals:
- URL changed to expected route
- a unique heading or main container became visible
- loading indicator disappeared
- toast/alert appeared
- previous modal/overlay disappeared
- new form controls appeared
- row count or grid content changed
- a disabled/enabled state changed
- focus moved into a new interactive region

Never conclude “done” just because time passed.

### Phase 5 — Overlay, modal, drawer, and popup verification

Do not use a narrow dialog-only oracle.

An interaction expected to open an overlay counts as OPENED if any of these are positively observed:
- `role=dialog` or `role=alertdialog` becomes visible
- a drawer, side panel, popover, flyout, menu, or other overlay container becomes visible
- `aria-expanded=true` and the controlled content becomes visible
- a new heading, form, or unique control appears in an overlay region
- a backdrop appears
- focus moves into the newly opened region
- body scroll lock or a page inert state changes
- the trigger changes state and a related container becomes interactable

Verification rules:
- First determine whether the overlay opened.
- Then determine whether the opened overlay is usable.
- Missing dialog semantics alone is NOT a failure.
- If the UI changed in a way that strongly suggests an overlay opened, but the semantics are unclear, mark OPENED and continue.
- If the overlay likely opened but usability cannot be proven, mark INCONCLUSIVE and collect more evidence.
- Only mark FAIL after:
  - up to 3 interaction attempts total, and
  - at least one additional measurement such as DOM snapshot, screenshot, console, or network evidence.

For close flows:
- verify the overlay disappears or becomes hidden
- verify focus returns sensibly
- verify the underlying page is usable again

### Phase 6 — Forms and validation

For form interactions:
- prefer semantic locators first
- after typing into a field, trigger blur when validation is expected
- verify validation on visible UI state, not time
- distinguish:
  - field-level validation
  - form-level validation
  - server-side save failure
  - navigation after save

When testing save flows, success requires a positive post-save oracle such as:
- success toast
- redirected route
- persisted value visible after refresh or reopen
- row/grid content updated
- button state changed to reflect saved record

For every successful create, edit, or destructive action:
- verify the result in current UI
- then verify persistence by refresh, reopen, re-query, or revisiting the source list/detail page
- do not count the action complete until persistence is verified or a blocker is recorded

### Phase 7 — Bug confirmation and triage

When you observe a suspected bug:
1. Capture the exact repro path.
2. Capture screenshot(s) immediately, before dismissing overlays, refreshing, or retrying, and inspect them for visible clues.
3. Capture any console/aspire logs/network evidence available.
4. Record expected vs actual.
5. Retry once by a different safe interaction path if possible.
6. If the retry changes the UI, capture a second screenshot of the new state.
7. Classify:
   - FAIL if contradiction is clear
   - INCONCLUSIVE if evidence is mixed
   - BLOCKED if environment/access prevents confirmation

Do not create a bug from a single ambiguous modal/open-state misread.

### Phase 8 — Stop conditions

Do not stop early.

You may stop only when one of these is true:
1. All original checklist items and all checklist items discovered during execution are resolved as PASS, FAIL, INCONCLUSIVE, or BLOCKED, no item remains TODO or IN_PROGRESS, and the remaining queue is empty.
2. A hard blocker prevents further progress and is recorded as BLOCKED.
3. The app is in a persistent broken state that prevents navigation and is documented with evidence.

Before stopping, explicitly review:
- remaining top-level sections
- remaining secondary navigation
- remaining checklist items
- partially explored flows
- open anomalies still marked INCONCLUSIVE
- all visible controls encountered
- all visited pages
- all opened dialogs
- all row action menus
- all tabs and subtabs
- all API operations exercised during the run

If any remain, continue unless BLOCKED.

## Evidence collection

For every confirmed finding record:
- title
- environment
- auth method
- route
- exact repro steps
- inputs used
- expected
- actual
- verdict
- screenshots
- visual inspection notes
- console/network notes if available
- whether any framework error overlay appeared

## Output format
Output everything as a files into folder AiTesting/{DATE}/{RUNID}/{Category}
Produce:
1. Compact exploratory plan/checklist
2. Coverage ledger
3. Findings grouped by FAIL / INCONCLUSIVE / BLOCKED
4. Remaining untested items
5. Artifact paths or links if saved
6. Screenshot index with each screenshot's trigger, route, and visual verdict

If no defect is confirmed, still report coverage and any INCONCLUSIVE areas.
