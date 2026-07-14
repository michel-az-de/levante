import { afterEach, beforeEach, describe, expect, it } from "vitest";
import { newsletterHabilitada, siteIndexavel } from "./flags";

const original = { ...process.env };

beforeEach(() => {
  delete process.env.SITE_INDEXABLE;
  delete process.env.NEWSLETTER_ENABLED;
});

afterEach(() => {
  process.env = { ...original };
});

describe("flags de operacao", () => {
  it("siteIndexavel: off por default, so 'true' habilita", () => {
    expect(siteIndexavel()).toBe(false);
    process.env.SITE_INDEXABLE = "false";
    expect(siteIndexavel()).toBe(false);
    process.env.SITE_INDEXABLE = "1";
    expect(siteIndexavel()).toBe(false);
    process.env.SITE_INDEXABLE = "true";
    expect(siteIndexavel()).toBe(true);
  });

  it("newsletterHabilitada: off por default, so 'true' habilita", () => {
    expect(newsletterHabilitada()).toBe(false);
    process.env.NEWSLETTER_ENABLED = "true";
    expect(newsletterHabilitada()).toBe(true);
  });
});
