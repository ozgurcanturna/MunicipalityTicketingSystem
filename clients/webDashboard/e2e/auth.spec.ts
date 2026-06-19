import { test, expect } from '@playwright/test';

test.describe('Authentication', () => {
  test('displays the login page', async ({ page }) => {
    await page.goto('/login');

    await expect(page.getByRole('heading', { name: 'Giriş Yap' })).toBeVisible();
    await expect(page.getByLabel('E-posta')).toBeVisible();
    await expect(page.getByLabel('Şifre')).toBeVisible();
    await expect(page.getByRole('button', { name: 'Giriş Yap' })).toBeVisible();
  });

  test('redirects to the dashboard after login', async ({ page, isMobile }) => {
    test.skip(isMobile, 'Dashboard layout not optimized for mobile viewports');

    await page.goto('/login');

    await page.getByLabel('E-posta').fill('admin@bursa.local');
    await page.getByLabel('Şifre').fill('P@ssw0rd!');
    await page.getByRole('button', { name: 'Giriş Yap' }).click();

    await expect(page).toHaveURL('http://localhost:3000/');
    await expect(page.getByRole('heading', { name: 'Özet' })).toBeVisible();
    await expect(page.getByText('Toplam Sefer')).toBeVisible();
  });
});