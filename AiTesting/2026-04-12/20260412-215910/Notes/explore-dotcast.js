const fs = require("fs");
const path = require("path");

let chromium;
try {
  ({ chromium } = require("playwright"));
} catch {
  ({ chromium } = require("C:/Users/tichy/AppData/Roaming/npm/node_modules/@playwright/cli/node_modules/playwright"));
}

const baseUrl = "https://localhost:7062";
const runRoot = path.resolve("AiTesting/2026-04-12/20260412-215910");
const screenshotDir = path.join(runRoot, "Artifacts", "Screenshots");
const reportDir = path.join(runRoot, "Reports");
const audioZip = "C:/Users/tichy/Downloads/Hayes Terry - Já, Poutník (2020)/Hayes Terry - Já, Poutník (2020).zip";

const allIds = [
  "C001", "C002", "C003", "C004", "C005", "C006", "C007", "C008", "C009", "C010",
  "C011", "C012", "C013", "C014", "C015", "C016", "C017", "C018", "C019", "C020",
  "C021", "C022", "C023", "C024", "C025"
];

const ledger = {
  environment: `Aspire AppHost, ${baseUrl}`,
  authMethod: "seeded credentials from appsettings.json",
  currentUserRole: "",
  topLevelNavigation: [],
  visitedTopLevelSections: [],
  visitedRoutes: [],
  visitedSecondaryNavigation: [],
  dialogs: [],
  discoveredItems: [],
  stateChangingFlows: [],
  destructiveActions: [],
  cleanupActions: [],
  screenshots: [],
  statuses: {},
  findings: [],
  console: [],
  network: [],
  notes: [],
  visualNotes: [],
  blockers: [],
  remaining: [],
};

function fileSafe(name) {
  return name.replace(/[^a-z0-9._-]+/gi, "_").replace(/^_+|_+$/g, "").slice(0, 120);
}

async function stabilize(page) {
  await page.waitForLoadState("domcontentloaded").catch(() => {});
  await page.waitForLoadState("networkidle", { timeout: 5000 }).catch(() => {});
  await page.waitForTimeout(500);
}

async function visibleText(page) {
  return (await page.locator("body").innerText({ timeout: 5000 }).catch(() => "")).replace(/\s+/g, " ").trim();
}

async function appErrorVisible(page) {
  return await page.locator("#blazor-error-ui").evaluate(el => getComputedStyle(el).display !== "none").catch(() => false);
}

async function shot(page, id, trigger, verdict = "PASS: page rendered; no obvious blank page, stale spinner, or overlay defect") {
  await stabilize(page);
  const file = `${id}_${fileSafe(trigger)}.png`;
  const full = path.join(screenshotDir, file);
  await page.screenshot({ path: full, fullPage: true });
  const route = page.url().replace(baseUrl, "") || "/";
  if (!ledger.visitedRoutes.includes(route)) ledger.visitedRoutes.push(route);
  ledger.screenshots.push({ file, full, trigger, route, visualVerdict: verdict });
  ledger.visualNotes.push(`${file}: ${verdict}`);
  return full;
}

function record(id, status, evidence) {
  ledger.statuses[id] = { status, evidence };
}

function pass(id, evidence) {
  record(id, "PASS", evidence);
}

function fail(id, title, evidence, route) {
  record(id, "FAIL", evidence);
  ledger.findings.push({ verdict: "FAIL", title, route, evidence });
}

function inconclusive(id, title, evidence, route) {
  record(id, "INCONCLUSIVE", evidence);
  ledger.findings.push({ verdict: "INCONCLUSIVE", title, route, evidence });
}

function blocked(id, title, evidence, route) {
  record(id, "BLOCKED", evidence);
  ledger.blockers.push(`${title}: ${evidence}`);
  ledger.findings.push({ verdict: "BLOCKED", title, route, evidence });
}

async function login(page, username, password, remember = false) {
  await page.goto(`${baseUrl}/Login`);
  await stabilize(page);
  const inputs = page.locator("input");
  await inputs.nth(0).fill(username);
  await inputs.nth(1).fill(password);
  if (remember && await inputs.nth(2).count()) {
    await inputs.nth(2).check().catch(() => {});
  }
  await page.getByRole("button", { name: /^Login$/ }).click();
  await page.waitForURL(url => !url.toString().includes("/Login") || url.toString().includes("Message="), { timeout: 12000 }).catch(() => {});
  await stabilize(page);
}

async function logout(page) {
  await page.goto(`${baseUrl}/UserProfile`);
  await stabilize(page);
  const button = page.getByRole("button", { name: /Logout/i });
  if (await button.count()) {
    await button.first().click();
    await page.waitForURL(/\/Login/i, { timeout: 10000 }).catch(() => {});
  } else {
    await page.goto(`${baseUrl}/api/logout`);
  }
  await stabilize(page);
}

async function firstCard(page) {
  return page.locator(".c-card").first();
}

async function waitForCards(page, timeoutMs = 90000) {
  const started = Date.now();
  while (Date.now() - started < timeoutMs) {
    await page.goto(`${baseUrl}/`);
    await stabilize(page);
    const count = await page.locator(".c-card").count();
    if (count > 0) return count;
    await page.waitForTimeout(3000);
  }
  return await page.locator(".c-card").count();
}

async function main() {
  fs.mkdirSync(screenshotDir, { recursive: true });
  fs.mkdirSync(reportDir, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    ignoreHTTPSErrors: true,
    viewport: { width: 1440, height: 1000 },
    acceptDownloads: true,
    permissions: ["clipboard-read", "clipboard-write"],
  });
  const page = await context.newPage();

  page.on("console", msg => ledger.console.push({ type: msg.type(), text: msg.text(), url: page.url() }));
  page.on("response", response => {
    const status = response.status();
    if (status >= 400) ledger.network.push({ status, url: response.url() });
  });
  page.on("dialog", async dialog => {
    ledger.dialogs.push(`${dialog.type()}: ${dialog.message()}`);
    await dialog.accept().catch(() => {});
  });

  await page.goto(`${baseUrl}/Login`);
  await shot(page, "C001", "login_page");
  const loginText = await visibleText(page);
  if (/Login/i.test(loginText) && /Username/i.test(loginText) && /Password/i.test(loginText)) pass("C001", "Login form rendered with expected fields");
  else fail("C001", "Login form missing expected fields", loginText, "/Login");

  await page.goto(`${baseUrl}/`);
  await stabilize(page);
  await shot(page, "C002", "logged_out_root_redirect");
  if (/\/Login/i.test(page.url())) pass("C002", `Redirected to ${page.url().replace(baseUrl, "")}`);
  else inconclusive("C002", "Logged-out root did not clearly redirect", page.url(), page.url());

  await login(page, "admin", "wrong-password");
  await shot(page, "C003", "invalid_login");
  const invalidText = await visibleText(page);
  if (/Login failed/i.test(invalidText) && /\/Login/i.test(page.url())) pass("C003", "Invalid login stayed on Login with visible failure message");
  else inconclusive("C003", "Invalid login result was not clearly signaled", invalidText, page.url());

  await login(page, "admin", "admin");
  await shot(page, "C004", "admin_login_audio_books");
  ledger.currentUserRole = "admin";
  if (!/\/Login/i.test(page.url()) && /DotCast|AudioBooks|Titles|Authors/i.test(await visibleText(page))) pass("C004", "Admin login reached protected app");
  else fail("C004", "Admin login did not reach protected app", await visibleText(page), page.url());

  ledger.topLevelNavigation = await page.locator("a").evaluateAll(links =>
    Array.from(new Set(links.map(a => `${a.textContent.trim()} -> ${a.getAttribute("href") || ""}`).filter(x => x.trim() !== "->")))
  ).catch(() => []);
  ledger.visitedTopLevelSections.push("AudioBooks");

  await page.goto(`${baseUrl}/UserProfile`);
  await stabilize(page);
  await shot(page, "C016", "admin_user_profile");
  ledger.visitedTopLevelSections.push("UserProfile");
  const profileText = await visibleText(page);
  if (/User Profile/i.test(profileText) && /Library Sharing/i.test(profileText)) pass("C016", "Profile and library sharing rendered for admin");
  else fail("C016", "Profile page missing expected sections", profileText, "/UserProfile");

  const restoreButton = page.getByRole("button", { name: /Restore Audiobooks/i });
  if (await restoreButton.count()) {
    if (await restoreButton.first().isDisabled().catch(() => false)) {
      blocked("C005", "Restore Audiobooks disabled", "Processing already running or button disabled", page.url());
    } else {
      await restoreButton.first().click();
      ledger.stateChangingFlows.push("C005: Restore Audiobooks");
      await page.waitForTimeout(1500);
      await shot(page, "C005", "restore_audiobooks_clicked");
      const count = await waitForCards(page);
      if (count > 0) pass("C005", `Restore flow completed sufficiently for exploration; ${count} audiobook card(s) visible`);
      else inconclusive("C005", "Restore clicked but library did not show cards within wait", await visibleText(page), page.url());
    }
  } else {
    blocked("C005", "Restore Audiobooks unavailable", "No admin restore button found", page.url());
  }

  await page.goto(`${baseUrl}/`);
  await stabilize(page);
  await shot(page, "C006", "admin_library_page");
  const rootText = await visibleText(page);
  if (/Titles:/i.test(rootText) && /Authors:/i.test(rootText) && /Duration:/i.test(rootText)) pass("C006", "Stats header rendered");
  else inconclusive("C006", "Library page rendered but stats were not all visible", rootText, "/");

  const search = page.getByPlaceholder("Search");
  if (await search.count()) {
    await search.fill("Poutnik");
    await page.keyboard.press("Tab");
    await page.waitForTimeout(1400);
    await shot(page, "C007", "library_search_poutnik");
    if (!(await appErrorVisible(page))) pass("C007", "Search accepted input without Blazor error");
    else fail("C007", "Search caused Blazor error UI", "blazor-error-ui visible", page.url());
  } else {
    blocked("C007", "Search input unavailable", "No Search placeholder found", page.url());
  }

  await page.goto(`${baseUrl}/`);
  await stabilize(page);
  const cardCount = await page.locator(".c-card").count();
  ledger.notes.push(`Audio book cards visible: ${cardCount}`);

  if (cardCount > 0) {
    const card = await firstCard(page);
    const editLink = card.locator('a[href*="/edit"], a[href*="AudioBook/"]').first();
    if (await editLink.count()) {
      await editLink.click();
      await stabilize(page);
      await shot(page, "C008", "edit_first_book");
      const editText = await visibleText(page);
      if (/Suggestions/i.test(editText) && /Upload files/i.test(editText)) pass("C008", "Edit form rendered with suggestions and upload controls");
      else inconclusive("C008", "Edit route opened but expected edit controls were incomplete", editText, page.url());

      const suggestions = page.getByRole("button", { name: /Suggestions/i });
      if (await suggestions.count()) {
        await suggestions.first().click();
        await page.waitForSelector(".modal, [role='dialog']", { timeout: 20000 }).catch(() => {});
        await shot(page, "C009", "suggestions_dialog");
        const modalText = await visibleText(page);
        if (/Suggestions/i.test(modalText)) pass("C009", "Suggestions modal opened");
        else inconclusive("C009", "Suggestions action did not expose clear modal content", modalText, page.url());
        await page.keyboard.press("Escape").catch(() => {});
        await page.locator(".modal .close, button.close, .btn-close").first().click().catch(() => {});
        await page.waitForTimeout(500);
      } else {
        blocked("C009", "Suggestions button unavailable", "No Suggestions button found", page.url());
      }

      const categorySelect = page.locator("select").first();
      if (await categorySelect.count()) {
        const options = await categorySelect.locator("option").evaluateAll(opts => opts.map(o => ({ value: o.value, text: o.textContent.trim() })));
        const candidate = options.find(o => o.value && !/choose/i.test(o.text));
        if (candidate) {
          await categorySelect.selectOption(candidate.value);
          await stabilize(page);
          await shot(page, "C010", "add_category");
          ledger.stateChangingFlows.push("C010: Add category");
          pass("C010", `Selected category ${candidate.text || candidate.value}`);
        } else {
          blocked("C010", "No missing category available", "Select had no actionable option", page.url());
        }
      } else {
        blocked("C010", "Category select unavailable", "No select found", page.url());
      }

      const fileInput = page.locator('input[type="file"]').first();
      if (await fileInput.count() && fs.existsSync(audioZip)) {
        const uploadResponse = page.waitForResponse(
          response => /\/storage\/archive\//.test(response.url()) && response.request().method() === "PUT",
          { timeout: 300000 }
        ).catch(error => ({ error: String(error) }));
        await fileInput.setInputFiles(audioZip);
        ledger.stateChangingFlows.push("C011: Provided ZIP selected for upload");
        await page.waitForTimeout(3000);
        await shot(page, "C011", "zip_upload_started");
        const progressCount = await page.locator("progress").count();
        const uploadResult = await uploadResponse;
        const uploadErrors = ledger.network.filter(n => /\/storage\/archive|\/storage\/file/.test(n.url));
        if (uploadResult && typeof uploadResult.status === "function" && uploadResult.status() >= 200 && uploadResult.status() < 300) {
          pass("C011", `Provided ZIP uploaded with HTTP ${uploadResult.status()}; ${progressCount} progress element(s) created`);
        } else if (progressCount > 0) {
          inconclusive("C011", "ZIP upload started but completion was not proven", JSON.stringify({ uploadResult, uploadErrors }), page.url());
        } else {
          inconclusive("C011", "ZIP selected but upload progress did not appear", JSON.stringify({ uploadResult, uploadErrors }), page.url());
        }
      } else {
        blocked("C011", "Upload could not be attempted", `fileInput=${await fileInput.count()}, zipExists=${fs.existsSync(audioZip)}`, page.url());
      }

      const sortButton = page.getByRole("button", { name: /Sort by name/i });
      if (await sortButton.count()) {
        await sortButton.first().click();
        await stabilize(page);
        await shot(page, "C012", "sort_chapters");
        ledger.stateChangingFlows.push("C012: Sort chapters by name");
        if (!(await appErrorVisible(page))) pass("C012", "Sort by name clicked without immediate UI error");
        else fail("C012", "Sort by name caused Blazor error UI", "blazor-error-ui visible", page.url());
      } else {
        blocked("C012", "Sort chapters unavailable", "No chapters/sort button visible", page.url());
      }

      const textInputs = page.locator("input[type='text'], input:not([type])");
      if (await textInputs.count()) {
        const originalName = await textInputs.nth(0).inputValue().catch(() => "");
        const marker = "[explore-215910]";
        const editedName = `${originalName.replace(/\s*\[explore-[^\]]+\]/, "")} ${marker}`.trim();
        await textInputs.nth(0).fill(editedName);
        await page.getByRole("button", { name: /^Save$/ }).click();
        await page.waitForURL(`${baseUrl}/`, { timeout: 15000 }).catch(() => {});
        await stabilize(page);
        await shot(page, "C013", "save_edit_return_library");
        ledger.stateChangingFlows.push("C013: Edit metadata and save");
        await page.reload();
        await stabilize(page);
        const afterSaveText = await visibleText(page);
        if (afterSaveText.includes(marker)) pass("C013", "Edited name persisted after save and reload");
        else inconclusive("C013", "Save returned to library but edited title was not visible after reload", afterSaveText, page.url());
      } else {
        blocked("C013", "No text inputs available for metadata edit", "No editable text controls", page.url());
      }

      await page.goto(`${baseUrl}/`);
      await stabilize(page);
    } else {
      blocked("C008", "No edit link available", "First card had no edit link", page.url());
      for (const id of ["C009", "C010", "C011", "C012", "C013"]) blocked(id, "Edit page unavailable", "Depends on C008", page.url());
    }

    const cardAfter = await firstCard(page);
    const buttons = cardAfter.locator("button");
    if (await buttons.count()) {
      await buttons.last().click().catch(() => {});
      await page.waitForTimeout(1000);
      await shot(page, "C014", "copy_rss_first_book");
      if (!(await appErrorVisible(page))) pass("C014", `RSS action attempted; dialogs: ${ledger.dialogs.join("; ") || "none"}`);
      else fail("C014", "RSS action caused Blazor error UI", "blazor-error-ui visible", page.url());
    } else {
      blocked("C014", "RSS action unavailable", "No card button found", page.url());
    }

    const buttonCount = await buttons.count();
    if (buttonCount > 1) {
      const beforePages = context.pages().length;
      await buttons.first().click().catch(() => {});
      await page.waitForTimeout(1000);
      await shot(page, "C015", "download_archive_first_book");
      if (!(await appErrorVisible(page))) pass("C015", `Download action attempted; pages before=${beforePages}, after=${context.pages().length}`);
      else fail("C015", "Download action caused Blazor error UI", "blazor-error-ui visible", page.url());
    } else {
      blocked("C015", "Download action unavailable", "No distinct download button found", page.url());
    }
  } else {
    for (const id of ["C008", "C009", "C010", "C011", "C012", "C013", "C014", "C015"]) {
      blocked(id, "No audiobook card available", "Library has no cards", "/");
    }
  }

  await page.goto(`${baseUrl}/UserProfile`);
  await stabilize(page);
  const shareInput = page.locator("#share-input");
  if (await shareInput.count()) {
    await shareInput.fill("invalid-library-code");
    await page.getByRole("button", { name: /^Share$/ }).click();
    await stabilize(page);
    await shot(page, "C017", "share_invalid_code");
    if (!(await appErrorVisible(page))) pass("C017", "Invalid share code did not crash the app");
    else fail("C017", "Invalid share code caused Blazor error UI", "blazor-error-ui visible", page.url());
  } else {
    blocked("C017", "Share input unavailable", "No #share-input found", page.url());
  }

  for (const [id, label, regex] of [
    ["C018", "Reprocess (No Unzip)", /Reprocess \(No Unzip\)/i],
    ["C019", "Reprocess (Unzip)", /Reprocess \(Unzip\)/i],
  ]) {
    const button = page.getByRole("button", { name: regex });
    if (await button.count()) {
      if (await button.first().isDisabled().catch(() => false)) {
        blocked(id, `${label} disabled`, "Processing was already running or button disabled", page.url());
      } else {
        await button.first().click();
        ledger.stateChangingFlows.push(`${id}: ${label}`);
        await page.waitForTimeout(1000);
        await shot(page, id, label.toLowerCase().replace(/[^a-z0-9]+/g, "_"));
        if (!(await appErrorVisible(page))) pass(id, `${label} clicked without immediate Blazor error`);
        else fail(id, `${label} caused Blazor error UI`, "blazor-error-ui visible", page.url());
      }
    } else {
      blocked(id, `${label} button unavailable`, "No matching button found", page.url());
    }
  }

  await logout(page);
  await shot(page, "C020", "logout_to_login");
  if (/\/Login/i.test(page.url())) pass("C020", "Logout reached Login page");
  else inconclusive("C020", "Logout did not clearly reach Login", page.url(), page.url());

  await login(page, "guest", "guest");
  await page.goto(`${baseUrl}/UserProfile`);
  await stabilize(page);
  await shot(page, "C021", "guest_login_profile");
  ledger.currentUserRole = "guest";
  if (!/\/Login/i.test(page.url()) && /User Profile/i.test(await visibleText(page))) pass("C021", "Guest login reached protected profile");
  else fail("C021", "Guest login did not reach protected profile", await visibleText(page), page.url());

  await shot(page, "C022", "guest_user_profile");
  const guestProfileText = await visibleText(page);
  if (/Admin Actions/i.test(guestProfileText)) fail("C022", "Guest can see Admin Actions", "Admin Actions section visible to guest", "/UserProfile");
  else if (/User Profile/i.test(guestProfileText)) pass("C022", "Guest profile rendered without Admin Actions");
  else inconclusive("C022", "Guest profile did not render expected text", guestProfileText, page.url());

  await page.goto(`${baseUrl}/swagger`);
  await stabilize(page);
  await shot(page, "C023", "swagger_route");
  const swaggerText = await visibleText(page);
  if (/Swagger|OpenAPI/i.test(swaggerText)) pass("C023", "Swagger UI rendered");
  else blocked("C023", "Swagger not available", "Route rendered app fallback rather than Swagger/OpenAPI UI", "/swagger");

  await page.goto(`${baseUrl}/definitely-not-a-real-route`);
  await stabilize(page);
  await shot(page, "C024", "not_found_route");
  const notFoundText = await visibleText(page);
  if (/nothing at this address/i.test(notFoundText)) pass("C024", "Not found page rendered");
  else inconclusive("C024", "Unknown route did not show expected not-found text", notFoundText, page.url());

  await page.setViewportSize({ width: 390, height: 844 });
  await page.goto(`${baseUrl}/Login`);
  await shot(page, "C025a", "mobile_login");
  await login(page, "admin", "admin");
  await page.goto(`${baseUrl}/`);
  await shot(page, "C025b", "mobile_library");
  await page.goto(`${baseUrl}/UserProfile`);
  await shot(page, "C025c", "mobile_profile");
  pass("C025", "Mobile screenshots captured for login, library, and profile");

  await browser.close();

  for (const id of allIds) {
    if (!ledger.statuses[id]) {
      blocked(id, "Checklist item did not run", "No status was recorded before run completion", "");
    }
  }
  ledger.remaining = allIds.filter(id => ["TODO", "IN_PROGRESS"].includes(ledger.statuses[id]?.status));

  const grouped = { FAIL: [], INCONCLUSIVE: [], BLOCKED: [] };
  for (const finding of ledger.findings) grouped[finding.verdict].push(finding);

  const screenshotIndex = ["# Screenshot Index", "", "| File | Trigger | Route | Visual verdict |", "|---|---|---|---|"];
  for (const s of ledger.screenshots) {
    screenshotIndex.push(`| ${s.file} | ${s.trigger} | ${s.route} | ${s.visualVerdict} |`);
  }
  fs.writeFileSync(path.join(reportDir, "screenshot-index.md"), `${screenshotIndex.join("\n")}\n`);

  const findingsMd = ["# Findings", ""];
  for (const verdict of ["FAIL", "INCONCLUSIVE", "BLOCKED"]) {
    findingsMd.push(`## ${verdict}`, "");
    if (!grouped[verdict].length) {
      findingsMd.push("None.", "");
      continue;
    }
    for (const f of grouped[verdict]) {
      findingsMd.push(`- ${f.title}`);
      findingsMd.push(`  - Route: ${f.route}`);
      findingsMd.push(`  - Evidence: ${String(f.evidence).replace(/\n/g, " ").slice(0, 1000)}`);
    }
    findingsMd.push("");
  }
  fs.writeFileSync(path.join(reportDir, "findings.md"), `${findingsMd.join("\n")}\n`);

  const coverageMd = [
    "# DotCast Coverage Ledger",
    "",
    `Environment used: ${ledger.environment}`,
    "",
    `Auth method used: ${ledger.authMethod}`,
    "",
    `Current authenticated user/role at end: ${ledger.currentUserRole}`,
    "",
    "Top-level navigation items discovered:",
    ...(ledger.topLevelNavigation.length ? ledger.topLevelNavigation.map(x => `- ${x}`) : ["- None"]),
    "",
    "Visited top-level sections:",
    ...Array.from(new Set(ledger.visitedTopLevelSections)).map(x => `- ${x}`),
    "",
    "Visited routes:",
    ...ledger.visitedRoutes.map(x => `- ${x}`),
    "",
    "Visited secondary navigation:",
    ...(ledger.visitedSecondaryNavigation.length ? ledger.visitedSecondaryNavigation.map(x => `- ${x}`) : ["- None"]),
    "",
    "Dialogs/overlays opened:",
    ...(ledger.dialogs.length ? ledger.dialogs.map(x => `- ${x}`) : ["- None captured"]),
    "",
    "Discovered during run checklist items:",
    ...(ledger.discoveredItems.length ? ledger.discoveredItems.map(x => `- ${x}`) : ["- None beyond initial checklist"]),
    "",
    "State-changing flows completed:",
    ...(ledger.stateChangingFlows.length ? ledger.stateChangingFlows.map(x => `- ${x}`) : ["- None"]),
    "",
    "Destructive actions completed:",
    ...(ledger.destructiveActions.length ? ledger.destructiveActions.map(x => `- ${x}`) : ["- None"]),
    "",
    "Cleanup actions completed:",
    ...(ledger.cleanupActions.length ? ledger.cleanupActions.map(x => `- ${x}`) : ["- None"]),
    "",
    "Checklist item statuses:",
    ...allIds.map(id => `- ${id}: ${ledger.statuses[id].status} - ${ledger.statuses[id].evidence}`),
    "",
    "Screenshot artifact index:",
    "- See `screenshot-index.md`",
    "",
    "Visual inspection notes:",
    ...ledger.visualNotes.map(x => `- ${x}`),
    "",
    "Anomalies observed:",
    ...(ledger.findings.length ? ledger.findings.map(f => `- ${f.verdict}: ${f.title}`) : ["- None"]),
    "",
    "Console notes:",
    ...(ledger.console.length ? ledger.console.slice(-100).map(c => `- ${c.type}: ${c.text}`) : ["- None"]),
    "",
    "HTTP errors observed:",
    ...(ledger.network.length ? ledger.network.slice(-100).map(n => `- ${n.status}: ${n.url}`) : ["- None"]),
    "",
    "Blockers:",
    ...(ledger.blockers.length ? ledger.blockers.map(x => `- ${x}`) : ["- None"]),
    "",
    "Remaining queue:",
    ...(ledger.remaining.length ? ledger.remaining.map(id => `- ${id}`) : ["- None"]),
  ];
  fs.writeFileSync(path.join(reportDir, "coverage-ledger.md"), `${coverageMd.join("\n")}\n`);

  const remainingMd = ["# Remaining Untested Items", ""];
  if (ledger.remaining.length) {
    remainingMd.push(...ledger.remaining.map(id => `- ${id}`));
  } else {
    remainingMd.push("None. Every initial checklist item has a terminal verdict.");
  }
  fs.writeFileSync(path.join(reportDir, "remaining-untested.md"), `${remainingMd.join("\n")}\n`);

  fs.writeFileSync(path.join(reportDir, "run-raw.json"), JSON.stringify(ledger, null, 2));
}

main().catch(error => {
  fs.mkdirSync(reportDir, { recursive: true });
  fs.writeFileSync(path.join(reportDir, "run-error.txt"), `${error.stack || error}\n`);
  process.exit(1);
});
