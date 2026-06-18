import { test, expect } from '@playwright/test';

/**
 * Dashboard page tests
 */
test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:3000');
    await page.waitForSelector('.grid', { timeout: 10000 });
  });

  test('Should display dashboard layout', async ({ page }) => {
    // Check sidebar navigation
    await expect(page.locator('[data-testid="sidebar"]')).toBeVisible();
    
    // Check main content area
    await expect(page.locator('.flex-1')).toBeVisible();
    
    // Check top navigation
    await expect(page.locator('[data-testid="top-nav"]')).toBeVisible();
  });

  test('Should navigate to buses page', async ({ page }) => {
    // Click on Buses navigation item
    await page.click('text=Büsler');
    await page.waitForTimeout(1000);
    
    // Verify buses page loaded
    await expect(page.locator('h1:has-text("Büsler")')).toBeVisible();
  });

  test('Should navigate to journeys page', async ({ page }) => {
    // Click on Seferler navigation item
    await page.click('text=Seferler');
    await page.waitForTimeout(1000);
    
    // Verify journeys page loaded
    await expect(page.locator('h1:has-text("Seferler")')).toBeVisible();
  });

  test('Should navigate to users page', async ({ page }) => {
    // Click on Kullanıcılar navigation item
    await page.click('text=Kullanıcılar');
    await page.waitForTimeout(1000);
    
    // Verify users page loaded
    await expect(page.locator('h1:has-text("Kullanıcılar")')).toBeVisible();
  });

  test('Should navigate to reports page', async ({ page }) => {
    // Click on Raporlar navigation item
    await page.click('text=Raporlar');
    await page.waitForTimeout(1000);
    
    // Verify reports page loaded
    await expect(page.locator('h1:has-text("Raporlar")')).toBeVisible();
  });

  test('Should navigate to settings page', async ({ page }) => {
    // Click on Ayarlar navigation item
    await page.click('text=Ayarlar');
    await page.waitForTimeout(1000);
    
    // Verify settings page loaded
    await expect(page.locator('h1:has-text("Ayarlar")')).toBeVisible();
  });
});