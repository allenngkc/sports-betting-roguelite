#!/usr/bin/env node
//
// register-scan.js — the DD seat's standing register scans, in one runnable file.
//
//   node tools/register-scan.js          (from the repo root)
//
// WHY THIS FILE EXISTS
// --------------------
// These four checks have been re-written from scratch at every DD seating, into a
// session-scratch directory that dies with the session. C22-am7 (batch 186) rules
// that the register must be reconciled FILES-TO-LOG-ENTRIES rather than by reading
// the log's tail. An instrument that does not survive its own session cannot be a
// standing check, so it lives here.
//
// It is also the validation gate the tracker-migration proposal's step 1 needs
// (docs/5-orchestration/tracking-brief-2026-08-25.md): an export must run against a
// register whose transcription backlog is ZERO, or it exports a partial world.
//
// Every check below is one this seat has actually run and had fire. Nothing here is
// aspirational: checks that could not be implemented faithfully were left out rather
// than stubbed, because a scan that reports HEALTHY without testing anything is the
// exact failure mode C29 made law against.
//
// Exit code is non-zero if any check fails, so it can gate a commit or an export.

const fs = require('fs');
const path = require('path');

const DIR = 'docs/design';
// Optional argv override, so the scan can be pointed at a snapshot to prove it FIRES.
// A check that has never been seen to fail is a vacuous green (C29).
const REG = process.argv[2] || path.join(DIR, 'REGISTER.md');

if (!fs.existsSync(REG)) {
  console.error('ABORT: ' + REG + ' not found — run this from the repo root.');
  process.exit(2);
}

const ID = '[A-Z]{1,2}\\d+(?:\\.\\d+)?(?:-[A-Za-z0-9]+)*';
const SECTIONS = ['SureThing', 'Room', 'TV', 'Cross-surface', 'Phone', 'Console'];

// A table cell split that honours the \| escape. Unescaped pipes inside a quoted
// ruling silently truncate the row — that defect deleted text from five rows on
// 2026-08-16, which is why cell-count is check 1 and not an afterthought.
function splitCells(inner) {
  const out = [];
  let cur = '';
  for (let i = 0; i < inner.length; i++) {
    const ch = inner[i];
    if (ch === '\\' && inner[i + 1] === '|') { cur += '\\|'; i++; continue; }
    if (ch === '|') { out.push(cur); cur = ''; continue; }
    cur += ch;
  }
  out.push(cur);
  return out;
}

const raw = fs.readFileSync(REG, 'utf8');
const L = raw.split(/\r?\n/);
let failures = 0;
const fail = (m) => { failures++; console.log('   FAIL  ' + m); };

// ---------------------------------------------------------------- 1. rows & cells
let section = null;
const rowsById = new Map();
const sectionCounts = {};
let rowCount = 0;

for (let i = 0; i < L.length; i++) {
  const h = L[i].match(/^## (.+)$/);
  if (h) {
    const name = h[1].split('—')[0].split('(')[0].trim();
    section = SECTIONS.includes(name) ? name : null;
    continue;
  }
  const line = L[i];
  if (!line.trimStart().startsWith('|')) continue;
  if (/^[\s|:\-]+$/.test(line)) continue;

  const cells = splitCells(line.trim().replace(/^\|/, '').replace(/\|$/, ''));
  const m = line.match(new RegExp('^\\|\\s*(' + ID + ')\\s*\\|'));
  if (!m) continue;

  rowCount++;
  if (section) sectionCounts[section] = (sectionCounts[section] || 0) + 1;

  if (cells.length !== 4) {
    fail('cell count ' + cells.length + ' (want 4) — line ' + (i + 1) + ', id ' + m[1] +
         '  <- almost always an UNESCAPED PIPE swallowing ruling text');
  }
  if (rowsById.has(m[1])) {
    fail('duplicate row id ' + m[1] + ' at lines ' + rowsById.get(m[1]) + ' and ' + (i + 1));
  } else {
    rowsById.set(m[1], i + 1);
  }
}

console.log('1. ROWS');
console.log('   id rows            : ' + rowCount);
console.log('   sections           : ' + JSON.stringify(sectionCounts));

// -------------------------------------------------- 2. definitions, inline-aware
// A ruling can be defined by a table row OR inline in another row's prose
// ("`G1-am5` — batch 60: ..."). C22-am3..am5 exist because an inline definition is
// invisible to a row-only scan, and nine IDs were issued twice before that was found.
const INLINE = new RegExp(
  '`?\\*{0,2}(' + ID + ')\\*{0,2}`?\\s*(?:·|—)\\s*batch\\s*(\\d+)' +
  '(?![\\w’\'])(?!\\s+(?:re-|ruled|says|said))', 'g');

// An id is defined ONCE per batch, however many times that batch is cited. Two prose
// mentions of "`T86-am` — batch 47" are one ruling referred to twice, not a collision;
// keying on (id, batch) rather than on the mention is what separates the two.
const defs = new Map();   // id -> Map(batchKey -> where)
const noteDef = (id, key, where) => {
  if (!defs.has(id)) defs.set(id, new Map());
  if (!defs.get(id).has(key)) defs.get(id).set(key, where);
};
for (const [id, ln] of rowsById) noteDef(id, 'row', 'row L' + ln);
// A row records the batch that made it in its last cell. When an inline definition
// cites that SAME batch, the two are one ruling written down twice (C22.1's case —
// promote the row, demote the prose to a cross-reference), not two rival rulings
// sharing an id. Different batches is the serious one. The check reports which.
const rowBatch = new Map();
for (const [id, ln] of rowsById) {
  const cells = splitCells(L[ln - 1].trim().replace(/^\|/, '').replace(/\|$/, ''));
  const b = (cells[3] || '').match(/batch\s*(\d+)/);
  if (b) rowBatch.set(id, +b[1]);
}
for (let i = 0; i < L.length; i++) {
  const t = L[i].replace(/\*\*/g, '');
  let x;
  INLINE.lastIndex = 0;
  while ((x = INLINE.exec(t))) noteDef(x[1], 'b' + x[2], 'inline b' + x[2] + ' L' + (i + 1));
}
const twice = [...defs].filter(([, w]) => w.size > 1).map(([id, w]) => [id, [...w.values()]]);
console.log('2. DEFINITIONS');
console.log('   ids defined        : ' + defs.size);
console.log('   defined more than once: ' + twice.length);
for (const [id, w] of twice) {
  const keys = [...defs.get(id).keys()];
  const inlineBatches = keys.filter((k) => k !== 'row').map((k) => +k.slice(1));
  // Three-way on purpose. About a quarter of rows carry a source path in the last
  // cell instead of a batch number, and for those the comparison cannot be made —
  // saying DIFFERENT there would report absent evidence as a finding.
  let kind;
  if (!keys.includes('row')) {
    kind = 'two inline definitions in different batches — re-key the later one';
  } else if (!rowBatch.has(id)) {
    kind = 'UNDETERMINED — the row\'s last cell carries no batch number, so this may be ' +
           'the same ruling twice or two rulings; read both sites before acting';
  } else if (inlineBatches.every((b) => b === rowBatch.get(id))) {
    kind = 'SAME batch as the row — one ruling written twice (C22.1: keep the row, ' +
           'make the prose a cross-reference)';
  } else {
    kind = 'DIFFERENT batches (row=' + rowBatch.get(id) + ', inline=' +
           inlineBatches.join('/') + ') — two rulings wearing one id; re-key the later';
  }
  fail('id ' + id + ' defined at ' + w.join(' AND ') + '\n         ' + kind);
}

// ------------------------------------------ 3. batch files reconciled to the log
// C22-am7: a gap in the MIDDLE is invisible from the newest end. Reading the log's
// tail reports HEALTHY while the register's middle is hollow — that is precisely
// how batches 163-172 stayed untranscribed while 173-181 were present.
const numOf = (f) => +f.match(/batch-(\d+)\.md$/)[1];
const batchFiles = fs.readdirSync(DIR)
  .filter((f) => /^register-entries-.*batch-\d+\.md$/.test(f))
  .sort((a, b) => numOf(a) - numOf(b));

const logged = new Set();
for (const m of raw.matchAll(/^- \*\*Batch (\d+) /gm)) logged.add(+m[1]);

// The log's own note records the batches that predate this convention and were
// deliberately NOT retrofitted; inventing entries for them would put false lines in
// a log whose entire value is that it is true. Parsed from the note, never hardcoded.
const noteMatch = raw.match(/Gap noted, deliberately NOT retrofitted: batches ([^)]*?)\s+have no entry/);
const exempt = new Set();
if (noteMatch) {
  for (const tok of noteMatch[1].split(/[,\s]+/)) {
    const r = tok.match(/^(\d+)[–-](\d+)$/);
    if (r) { for (let n = +r[1]; n <= +r[2]; n++) exempt.add(n); }
    else if (/^\d+$/.test(tok)) exempt.add(+tok);
  }
}

const missing = [];
for (const f of batchFiles) {
  const n = numOf(f);
  if (logged.has(n) || exempt.has(n)) continue;
  // A batch that carries no id-bearing rows owes the log nothing.
  const body = fs.readFileSync(path.join(DIR, f), 'utf8');
  const bears = body.split(/\r?\n/).some((l) =>
    l.trimStart().startsWith('|') && new RegExp('^\\|\\s*`?(' + ID + ')`?\\s*\\|').test(l.trim()));
  if (bears) missing.push(n);
}

console.log('3. BACKLOG (files reconciled to log entries)');
console.log('   batch files        : ' + batchFiles.length +
            '  (range ' + numOf(batchFiles[0]) + '-' + numOf(batchFiles[batchFiles.length - 1]) + ')');
console.log('   exempt by the log\'s own note: ' + [...exempt].sort((a, b) => a - b).join(', '));
console.log('   UNTRANSCRIBED      : ' + missing.length + (missing.length ? '  -> ' + missing.join(', ') : ''));
for (const n of missing) fail('batch ' + n + ' carries rows but has no transcription-log entry');

// ------------------------------------------------------------------- 4. verdict
console.log('');
if (failures === 0) {
  console.log('PROBLEMS: 0 — register is internally consistent and the backlog is zero.');
} else {
  console.log('PROBLEMS: ' + failures);
}
process.exit(failures === 0 ? 0 : 1);
