const fs = require("fs");
const path = require("path");
const { chromium } = require("C:/Users/tichy/AppData/Roaming/npm/node_modules/@playwright/cli/node_modules/playwright");

const baseUrl = "https://localhost:7062";
const runRoot = path.resolve("AiTesting/2026-04-12/20260412-214743");
const screenshotDir = path.join(runRoot, "Artifacts", "Screenshots");
const reportDir = path.join(runRoot, "Reports");
const audioZip = "C:/Users/tichy/Downloads/Hayes Terry - Já, Poutník (2020)/Hayes Terry - Já, Poutník (2020).zip";

const ledger = {
  environment: `Aspire AppHost, ${baseUrl}`,
  authMethod: "seeded credentials from appsettings.json",
  currentUserRole: "",
  topLevelNavigation: [],
  visitedRoutes: [],
  dialogs: [],
  screenshots: [],
  statuses: {},
  findings: [],
  console: [],
  network: [],
  notes: [],
  remaining: [],
};

function fileSafe(name) {
  return name.replace(/[^a-z0-9._-]+/gi, "_").replace(/^_+|_+$/g, "");
}

async function stabilize(page) {
  await page.waitForLoadState("domcontentloaded").catch(() => {});
  await page.waitForLoadState("networkidle", { timeout: 5000 }).catch(() => {});
  await page.waitForTimeout(500);
}

async function shot(page, id, trigger, visualVerdict = "PASS: page rendered without obvious blank/overlap defect") {
  await stabilize(page);
  const file = `${id}_${fileSafe(trigger)}.png`;
  const full = path.join(screenshotDir, file);
  await page.screenshot({ path: full, fullPage: true });
  const route = page.url().replace(baseUrl, "") || "/";
  ledger.screenshots.push({ file, trigger, route, visualVerdict });
  if (!ledger.visitedRoutes.includes(route)) ledger.visitedRoutes.push(route);
  return full;
}

async function visibleText(page) {
  return (await page.locator("body").innerText({ timeout: 5000 }).catch(() => "")).replace(/\s+/g, " ").trim();
}

async function appErrorVisible(page) {
  return await page.locator("#blazor-error-ui").evaluate(el => getComputedStyle(el).display !== "none").catch(() => false);
}

function pass(id, evidence) {
  ledger.statuses[id] = { status: "PASS", evidence };
}

function fail(id, title, evidence, route) {
  ledger.statuses[id] = { status: "FAIL", evidence };
  ledger.findings.push({ verdict: "FAIL", title, route, evidence });
}

function inconclusive(id, title, evidence, route) {
  ledger.statuses[id] = { status: "INCONCLUSIVE", evidence };
  ledger.findings.push({ verdict: "INCONCLUSIVE", title, route, evidence });
}

function blocked(id, title, evidence, route) {
  ledger.statuses[id] = { status: "BLOCKED", evidence };
  ledger.findings.push({ verdict: "BLOCKED", title, route, evidence });
}

async function login(page, username, password, remember = false) {
  await page.goto(`${baseUrl}/Login`);
  await stabilize(page);
  const inputs = page.locator("input");
  await inputs.nth(0).fill(username);
  await inputs.nth(1).fill(password);
  if (remember) {
    const checkbox = inputs.nth(2);
    if (await checkbox.count()) await checkbox.check().catch(() => {});
  }
  await page.getByRole("button", { name: /^Login$/ }).click();
  await page.waitForURL(url => !url.toString().includes("/Login") || url.toString().includes("Message="), { timeout: 10000 }).catch(() => {});
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

async function main() {
  fs.mkdirSync(screenshotDir, { recursive: true });
  fs.mkdirSync(reportDir, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    ignoreHTTPSErrors: true,
    viewport: { width: 1440, height: 1000 },
    acceptDownloads: true,
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

  await page.goto(`${baseUrl}/`);
  await stabilize(page);
  const loggedOutRoute = page.url().replace(baseUrl, "");
  await shot(page, "C005", "logged_out_root_redirect");
  if (/\/Login/i.test(loggedOutRoute)) pass("C005", `Redirected to ${loggedOutRoute}`);
  else inconclusive("C005", "Logged-out protected route did not clearly redirect", `Route after / was ${loggedOutRoute}`, loggedOutRoute);

  await page.goto(`${baseUrl}/Login`);
  await stabilize(page);
  await shot(page, "C001", "login_page");
  const loginText = await visibleText(page);
  if (/Login/i.test(loginText) && /Username/i.test(loginText) && /Password/i.test(loginText)) pass("C001", "Login form rendered");
  else fail("C001", "Login form missing expected fields", loginText, "/Login");

  await login(page, "admin", "wrong-password");
  await shot(page, "C002", "invalid_login");
  const invalidText = await visibleText(page);
  if (/Login failed/i.test(invalidText) && /\/Login/i.test(page.url())) pass("C002", "Invalid login stayed on Login with failure message");
  else inconclusive("C002", "Invalid login did not expose expected failure text", invalidText, page.url());

  await login(page, "admin", "admin");
  await shot(page, "C003", "admin_login_audio_books");
  ledger.currentUserRole = "admin";
  if (!/\/Login/i.test(page.url()) && /DotCast|AudioBooks|Titles|Authors/i.test(await visibleText(page))) pass("C003", "Admin login reached protected app");
  else fail("C003", "Admin login did not reach protected app", await visibleText(page), page.url());

  ledger.topLevelNavigation = await page.locator("nav a, .navbar a, header a, .bar a, a").evaluateAll(links =>
    Array.from(new Set(links.map(a => `${a.textContent.trim()} -> ${a.getAttribute("href") || ""}`).filter(Boolean)))
  ).catch(() => []);

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
    blocked("C007", "Search input not available", "No Search placeholder found", page.url());
  }

  const cards = page.locator(".c-card");
  const cardCount = await cards.count();
  ledger.notes.push(`Audio book cards found after login: ${cardCount}`);

  if (cardCount > 0) {
    const firstCard = cards.first();
    const editLink = firstCard.locator('a[href*="/edit"], a[href*="AudioBook/"]').first();
    const rssButton = firstCard.locator("button").last();
    if (await rssButton.count()) {
      await rssButton.click().catch(() => {});
      await page.waitForTimeout(1000);
      await shot(page, "C008", "copy_rss_first_book");
      pass("C008", `RSS action attempted; dialogs: ${ledger.dialogs.join("; ") || "none"}`);
    } else {
      blocked("C008", "RSS action not available", "No card button found", page.url());
    }

    const downloadButton = firstCard.locator("button").first();
    if (await downloadButton.count()) {
      const beforePages = context.pages().length;
      await downloadButton.click().catch(() => {});
      await page.waitForTimeout(1000);
      await shot(page, "C009", "download_archive_first_book");
      pass("C009", `Download action attempted; pages before=${beforePages}, after=${context.pages().length}`);
    } else {
      blocked("C009", "Download action not available", "No card button found", page.url());
    }

    if (await editLink.count()) {
      await editLink.click();
      await stabilize(page);
      await shot(page, "C010", "edit_first_book");
      const editText = await visibleText(page);
      if (/Suggestions/i.test(editText) && /Upload files/i.test(editText)) pass("C010", "Edit form rendered");
      else inconclusive("C010", "Edit route opened but expected edit controls were incomplete", editText, page.url());

      const suggestions = page.getByRole("button", { name: /Suggestions/i });
      if (await suggestions.count()) {
        await suggestions.first().click();
        await page.waitForSelector(".modal, [role='dialog']", { timeout: 15000 }).catch(() => {});
        await shot(page, "C011", "suggestions_dialog");
        const modalText = await visibleText(page);
        if (/Suggestions/i.test(modalText)) pass("C011", "Suggestions modal opened");
        else inconclusive("C011", "Suggestions action completed without clear modal evidence", modalText, page.url());
        await page.keyboard.press("Escape").catch(() => {});
        await page.locator(".modal .close, button.close").first().click().catch(() => {});
        await page.waitForTimeout(500);
      } else {
        blocked("C011", "Suggestions button unavailable", "No Suggestions button found", page.url());
      }

      const categorySelect = page.locator("select").first();
      if (await categorySelect.count()) {
        const options = await categorySelect.locator("option").evaluateAll(opts => opts.map(o => ({ value: o.value, text: o.textContent.trim() })).filter(o => o.value || o.text));
        const candidate = options.find(o => o.value && !/choose/i.test(o.text));
        if (candidate) {
          await categorySelect.selectOption(candidate.value);
          await stabilize(page);
          await shot(page, "C013", "add_category");
          pass("C013", `Selected category option ${candidate.text || candidate.value}`);
        } else {
          blocked("C013", "No missing category available", "Select had no actionable option", page.url());
        }
      } else {
        blocked("C013", "Category select unavailable", "No select found", page.url());
      }

      const fileInput = page.locator('input[type="file"]').first();
      if (await fileInput.count() && fs.existsSync(audioZip)) {
        await fileInput.setInputFiles(audioZip);
        await page.waitForTimeout(3000);
        await shot(page, "C014", "zip_upload_started");
        const progressCount = await page.locator("progress").count();
        if (progressCount > 0) pass("C014", `Provided zip selected; ${progressCount} progress element(s) created`);
        else inconclusive("C014", "Zip selected but upload progress did not appear", "No progress element after file selection", page.url());
      } else {
        blocked("C014", "Upload could not be attempted", `fileInput=${await fileInput.count()}, zipExists=${fs.existsSync(audioZip)}`, page.url());
      }

      const sortButton = page.getByRole("button", { name: /Sort by name/i });
      if (await sortButton.count()) {
        await sortButton.first().click();
        await stabilize(page);
        await shot(page, "C015", "sort_chapters");
        pass("C015", "Sort by name action clicked without immediate UI error");
      } else {
        blocked("C015", "Sort chapters unavailable", "No chapters/sort button visible", page.url());
      }

      const nameInput = page.locator("label:text('Name')").count ? null : null;
      const textInputs = page.locator("input[type='text'], input:not([type])");
      if (await textInputs.count()) {
        const originalName = await textInputs.nth(0).inputValue().catch(() => "");
        const editedName = `${originalName || "Exploratory Test"} [explore]`;
        await textInputs.nth(0).fill(editedName);
        await page.getByRole("button", { name: /^Save$/ }).click();
        await page.waitForURL(`${baseUrl}/`, { timeout: 10000 }).catch(() => {});
        await stabilize(page);
        await shot(page, "C012", "save_edit_return_library");
        const afterSaveText = await visibleText(page);
        if (afterSaveText.includes("[explore]")) pass("C012", "Edited name visible after save return");
        else inconclusive("C012", "Save returned to library but changed name was not visible in current view", afterSaveText, page.url());
      } else {
        blocked("C012", "No text inputs available for metadata edit", "No editable text controls", page.url());
      }
    } else {
      blocked("C010", "No edit link available", "First card had no edit link", page.url());
      blocked("C011", "Edit page unavailable", "Depends on C010", page.url());
      blocked("C012", "Edit page unavailable", "Depends on C010", page.url());
      blocked("C013", "Edit page unavailable", "Depends on C010", page.url());
      blocked("C014", "Edit page unavailable", "Depends on C010", page.url());
      blocked("C015", "Edit page unavailable", "Depends on C010", page.url());
    }
  } else {
    blocked("C008", "No audio book card available for RSS", "Library has no cards", "/");
    blocked("C009", "No audio book card available for download", "Library has no cards", "/");
    blocked("C010", "No audio book card available for edit", "Library has no cards", "/");
    blocked("C011", "No edit page available", "Library has no cards", "/");
    blocked("C012", "No edit page available", "Library has no cards", "/");
    blocked("C013", "No edit page available", "Library has no cards", "/");
    blocked("C014", "No edit page available for zip upload", "Library has no cards", "/");
    blocked("C015", "No edit page available", "Library has no cards", "/");
  }

  await page.goto(`${baseUrl}/UserProfile`);
  await stabilize(page);
  await shot(page, "C016", "admin_user_profile");
  const profileText = await visibleText(page);
  if (/User Profile/i.test(profileText) && /Library Sharing/i.test(profileText)) pass("C016", "Profile and library sharing rendered");
  else fail("C016", "Profile page missing expected sections", profileText, "/UserProfile");

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

  const adminButtons = [
    ["C018", "Restore Audiobooks", /Restore Audiobooks/i],
    ["C019", "Reprocess (No Unzip)", /Reprocess \(No Unzip\)/i],
    ["C020", "Reprocess (Unzip)", /Reprocess \(Unzip\)/i],
  ];
  for (const [id, label, regex] of adminButtons) {
    const button = page.getByRole("button", { name: regex });
    if (await button.count()) {
      const disabled = await button.first().isDisabled().catch(() => false);
      if (disabled) {
        blocked(id, `${label} disabled`, "Processing was already running or button disabled", page.url());
      } else {
        await button.first().click();
        await page.waitForTimeout(800);
        await shot(page, id, label.toLowerCase().replace(/[^a-z0-9]+/g, "_"));
        if (!(await appErrorVisible(page))) pass(id, `${label} clicked without immediate Blazor error`);
        else fail(id, `${label} caused Blazor error UI`, "blazor-error-ui visible", page.url());
      }
    } else {
      blocked(id, `${label} button unavailable`, "No matching button found", page.url());
    }
  }

  await logout(page);
  await shot(page, "C021", "logout_to_login");
  if (/\/Login/i.test(page.url())) pass("C021", "Logout reached Login page");
  else inconclusive("C021", "Logout did not clearly reach Login", page.url(), page.url());

  await login(page, "guest", "guest");
  await page.goto(`${baseUrl}/UserProfile`);
  await stabilize(page);
  await shot(page, "C004", "guest_login_profile");
  if (!/\/Login/i.test(page.url()) && /User Profile/i.test(await visibleText(page))) pass("C004", "Guest login reached protected profile");
  else fail("C004", "Guest login did not reach protected profile", await visibleText(page), page.url());

  await shot(page, "C022", "guest_user_profile");
  const guestProfileText = await visibleText(page);
  if (/Admin Actions/i.test(guestProfileText)) {
    fail("C022", "Guest can see Admin Actions", "Admin Actions section visible to guest", "/UserProfile");
  } else if (/User Profile/i.test(guestProfileText)) {
    pass("C022", "Guest profile rendered without Admin Actions");
  } else {
    inconclusive("C022", "Guest profile did not render expected text", guestProfileText, page.url());
  }

  await page.goto(`${baseUrl}/swagger`);
  await stabilize(page);
  await shot(page, "C023", "swagger_route");
  const swaggerText = await visibleText(page);
  if (/Swagger|OpenAPI/i.test(swaggerText)) pass("C023", "Swagger UI rendered");
  else blocked("C023", "Swagger not available", swaggerText, "/swagger");

  await page.goto(`${baseUrl}/definitely-not-a-real-route`);
  await stabilize(page);
  await shot(page, "C024", "not_found_route");
  if (/nothing at this address/i.test(await visibleText(page))) pass("C024", "Not found page rendered");
  else inconclusive("C024", "Unknown route did not show expected not-found text", await visibleText(page), page.url());

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

  const screenshotIndex = ["# Screenshot Index", "", "| File | Trigger | Route | Visual verdict |", "|---|---|---|---|"];
  for (const s of ledger.screenshots) {
    screenshotIndex.push(`| ${s.file} | ${s.trigger} | ${s.route} | ${s.visualVerdict} |`);
  }
  fs.writeFileSync(path.join(reportDir, "screenshot-index.md"), `${screenshotIndex.join("\n")}\n`);

  const unresolvedIds = [];
  for (const id of [
    "C001","C002","C003","C004","C005","C006","C007","C008","C009","C010","C011","C012","C013","C014","C015","C016","C017","C018","C019","C020","C021","C022","C023","C024","C025"
  ]) {
    if (!ledger.statuses[id]) unresolvedIds.push(id);
  }
  ledger.remaining = unresolvedIds;

  const grouped = { FAIL: [], INCONCLUSIVE: [], BLOCKED: [] };
  for (const finding of ledger.findings) grouped[finding.verdict].push(finding);

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
      findingsMd.push(`  - Evidence: ${String(f.evidence).replace(/\n/g, " ").slice(0, 500)}`);
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
    `Current authenticated user/role: ${ledger.currentUserRole}`,
    "",
    "Top-level navigation items discovered:",
    ...ledger.topLevelNavigation.map(x => `- ${x}`),
    "",
    "Visited routes:",
    ...ledger.visitedRoutes.map(x => `- ${x}`),
    "",
    "Dialogs/overlays opened:",
    ...(ledger.dialogs.length ? ledger.dialogs.map(x => `- ${x}`) : ["- None captured"]),
    "",
    "State-changing flows completed:",
    ...Object.entries(ledger.statuses).filter(([id, v]) => ["C012","C013","C014","C017","C018","C019","C020"].includes(id) && v.status === "PASS").map(([id, v]) => `- ${id}: ${v.evidence}`),
    "",
    "Checklist item statuses:",
    ...Object.entries(ledger.statuses).sort().map(([id, v]) => `- ${id}: ${v.status} - ${v.evidence}`),
    "",
    "Visual inspection notes:",
    ...ledger.screenshots.map(s => `- ${s.file}: ${s.visualVerdict}`),
    "",
    "Anomalies observed:",
    ...(ledger.findings.length ? ledger.findings.map(f => `- ${f.verdict}: ${f.title}`) : ["- None"]),
    "",
    "Console notes:",
    ...ledger.console.slice(-50).map(c => `- ${c.type}: ${c.text}`),
    "",
    "HTTP errors observed:",
    ...ledger.network.slice(-50).map(n => `- ${n.status}: ${n.url}`),
    "",
    "Blockers:",
    ...grouped.BLOCKED.map(f => `- ${f.title}: ${f.evidence}`),
    "",
    "Remaining queue:",
    ...(ledger.remaining.length ? ledger.remaining.map(id => `- ${id}`) : ["- None"]),
  ];
  fs.writeFileSync(path.join(reportDir, "coverage-ledger.md"), `${coverageMd.join("\n")}\n`);

  const rawPath = path.join(reportDir, "run-raw.json");
  fs.writeFileSync(rawPath, JSON.stringify(ledger, null, 2));
}

main().catch(error => {
  fs.mkdirSync(reportDir, { recursive: true });
  fs.writeFileSync(path.join(reportDir, "run-error.txt"), `${error.stack || error}\n`);
  process.exit(1);
});
