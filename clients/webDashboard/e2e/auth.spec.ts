import { test, expect } from '@playwright/test';

/**
 * Authentication flow tests
 */
test.describe('Authentication', () => {
  test.beforeEach(async ({ page }) => {
    // Start fresh state
    await page.goto('http://localhost:3000');
    await page.waitForSelector('[data-testid="dashboard-provider"]', { timeout: 10000 }).catch(() => {
      await page.waitForSelector('.grid', { timeout: 10000 }).catch(() => {});
    });
  });

  test('Should display login page', async ({ page }) => {
    // Check if login page is visible
    await page.waitForSelector('.min-h-screen', { timeout: 5000 });
    
    // Verify login form exists
    await expect(page.locator('form')).toBeVisible();
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('Should redirect to dashboard after login', async ({ page }) => {
    // Fill in mock credentials (will fail until backend is connected)
    await page.locator('input[type="email"]').fill('admin@municipality.gov.tr');
    await page.locator('input[type="password"]').fill('admin123');
    await page.locator('button[type="submit"]').click();
    
    // Should redirect to dashboard
    await page.waitForTimeout(2000);
    
    // Should now be on dashboard (even if login fails with mock data)
    await expect(page).toHaveURL(/dashboard/);
  });
});