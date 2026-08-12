import { Invoice, InvoiceStatus } from '../models/invoice.model';

export interface ComputedInvoice {
  inv: Invoice;
  construction: number;
  services: number;
  total: number;
  lowConfidenceCount: number;
  status: InvoiceStatus;
  tagClass: string;
  confidenceSummary: string;
  confidenceTagClass: string;
}

// Ported from the mockup's computeInvoice(). Status precedence, in order:
// any overridden line short-circuits to Reviewed regardless of confidence,
// otherwise >=2 low-confidence lines is Flagged, exactly 1 is Needs review,
// else Reviewed. confidenceSummary/confidenceTagClass are derived purely
// from lowConfidenceCount, independent of status/override, so they can
// visibly disagree with the status badge (e.g. Reviewed via override while
// confidence still reads "1 flagged"). That disagreement is intentional,
// carried over from the mockup, not a bug to fix.
export function computeInvoice(inv: Invoice): Omit<ComputedInvoice, 'inv'> {
  const construction = inv.lines
    .filter((l) => l.classification === 'construction')
    .reduce((sum, l) => sum + l.amount, 0);
  const services = inv.lines
    .filter((l) => l.classification === 'services')
    .reduce((sum, l) => sum + l.amount, 0);
  const total = construction + services;
  const lowConfidenceCount = inv.lines.filter((l) => l.confidence === 'low').length;
  const anyOverridden = inv.lines.some((l) => l.overridden);

  let status: InvoiceStatus;
  if (anyOverridden) status = 'Reviewed';
  else if (lowConfidenceCount >= 2) status = 'Flagged';
  else if (lowConfidenceCount === 1) status = 'Needs review';
  else status = 'Reviewed';

  const tagClass =
    status === 'Flagged' ? 'tag tag-accent' : status === 'Needs review' ? 'tag tag-outline' : 'tag tag-neutral';
  const confidenceSummary = lowConfidenceCount === 0 ? 'All high' : `${lowConfidenceCount} flagged`;
  const confidenceTagClass = lowConfidenceCount === 0 ? 'tag tag-neutral' : 'tag tag-accent';

  return { construction, services, total, lowConfidenceCount, status, tagClass, confidenceSummary, confidenceTagClass };
}

export function computeAll(invoices: Invoice[]): ComputedInvoice[] {
  return invoices.map((inv) => ({ inv, ...computeInvoice(inv) }));
}

export function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}

// --- Dashboard aggregates ---

export interface TrendBar {
  label: string;
  consH: number;
  servH: number;
}

export interface DashboardAggregates {
  totalAll: number;
  constructionAll: number;
  servicesAll: number;
  pct: number;
  servicesPct: number;
  invoiceCount: number;
  needsReviewCount: number;
  deductionGbp: number;
  cisRatePercent: number;
  trend: TrendBar[];
  reviewRows: ComputedInvoice[];
}

function monthKey(dateIso: string): string {
  return dateIso.slice(0, 7);
}

function monthLabel(key: string): string {
  return new Date(`${key}-01T00:00:00`).toLocaleDateString('en-GB', { month: 'short' });
}

// The trend chart's bar container is 108px tall (see dashboard.component.css).
// The mockup scaled bar height against *100 instead of the container's real
// height, which under-fills it by ~8%, fixed here to scale against the
// actual constant.
const TREND_BAR_HEIGHT_PX = 108;

export function buildDashboardAggregates(computed: ComputedInvoice[], cisRatePercent = 20): DashboardAggregates {
  const totalAll = computed.reduce((sum, c) => sum + c.total, 0);
  const constructionAll = computed.reduce((sum, c) => sum + c.construction, 0);
  const servicesAll = totalAll - constructionAll;
  const pct = totalAll ? Math.round((constructionAll / totalAll) * 100) : 0;
  const servicesPct = 100 - pct;
  const needsReviewCount = computed.filter((c) => c.status !== 'Reviewed').length;

  const monthMap = new Map<string, { cons: number; serv: number }>();
  for (const c of computed) {
    const key = monthKey(c.inv.date);
    const entry = monthMap.get(key) ?? { cons: 0, serv: 0 };
    entry.cons += c.construction;
    entry.serv += c.services;
    monthMap.set(key, entry);
  }
  const months = [...monthMap.keys()].sort();
  const maxCombined = Math.max(1, ...months.map((m) => monthMap.get(m)!.cons + monthMap.get(m)!.serv));
  const trend: TrendBar[] = months.map((m) => {
    const entry = monthMap.get(m)!;
    return {
      label: monthLabel(m),
      consH: Math.round((entry.cons / maxCombined) * TREND_BAR_HEIGHT_PX),
      servH: Math.round((entry.serv / maxCombined) * TREND_BAR_HEIGHT_PX),
    };
  });

  const reviewRows = computed.filter((c) => c.status !== 'Reviewed').slice(0, 4);

  return {
    totalAll, constructionAll, servicesAll, pct, servicesPct,
    invoiceCount: computed.length,
    needsReviewCount,
    deductionGbp: constructionAll * (cisRatePercent / 100),
    cisRatePercent,
    trend,
    reviewRows,
  };
}

// --- Grid filter/sort/search ---

export type SortKey = 'date' | 'supplier' | 'ref' | 'total' | 'construction' | 'services';
export type SortDir = 'asc' | 'desc';

function sortValue(c: ComputedInvoice, key: SortKey): string | number {
  switch (key) {
    case 'date': return c.inv.date;
    case 'supplier': return c.inv.supplier;
    case 'ref': return c.inv.ref;
    case 'total': return c.total;
    case 'construction': return c.construction;
    case 'services': return c.services;
  }
}

export function buildGridRows(
  computed: ComputedInvoice[],
  search: string,
  statusFilter: 'all' | InvoiceStatus,
  sortKey: SortKey,
  sortDir: SortDir,
): ComputedInvoice[] {
  let rows = computed.slice();

  if (statusFilter !== 'all') {
    rows = rows.filter((c) => c.status === statusFilter);
  }

  const query = search.trim().toLowerCase();
  if (query) {
    rows = rows.filter(
      (c) => c.inv.supplier.toLowerCase().includes(query) || c.inv.ref.toLowerCase().includes(query),
    );
  }

  rows.sort((a, b) => {
    const av = sortValue(a, sortKey);
    const bv = sortValue(b, sortKey);
    const cmp = av < bv ? -1 : av > bv ? 1 : 0;
    return sortDir === 'asc' ? cmp : -cmp;
  });

  return rows;
}
