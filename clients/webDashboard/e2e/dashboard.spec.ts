import { test, expect } from '@playwright/test';

async function login(page: import('@playwright/test').Page) {
  await page.goto('/login');
  await page.getByLabel('E-posta').fill('admin@bursa.local');
  await page.getByLabel('Şifre').fill('P@ssw0rd!');
  await page.getByRole('button', { name: 'Giriş Yap' }).click();
  await expect(page).toHaveURL('http://localhost:3000/');
  await expect(page.getByRole('heading', { name: 'Özet' })).toBeVisible();
}

test.describe('Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('displays the dashboard layout', async ({ page }) => {
    await expect(page.getByRole('navigation', { name: 'Ana menü' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Özet' })).toBeVisible();
    await expect(page.getByText('Bursa Admin')).toBeVisible();
  });

  test('navigates to the buses page', async ({ page }) => {
    await page.getByRole('link', { name: 'Otobüsler' }).click();
    await expect(page).toHaveURL('http://localhost:3000/buses');
    await expect(page.getByRole('heading', { name: 'Otobüs Yönetimi' })).toBeVisible();
  });

  test('navigates to the journeys page', async ({ page }) => {
    await page.getByRole('link', { name: 'Seferler' }).click();
    await expect(page).toHaveURL('http://localhost:3000/journeys');
    await expect(page.getByRole('heading', { name: 'Sefer Yönetimi' })).toBeVisible();
  });

  test('navigates to the users page', async ({ page }) => {
    await page.getByRole('link', { name: 'Kullanıcılar' }).click();
    await expect(page).toHaveURL('http://localhost:3000/users');
    await expect(page.getByRole('heading', { name: 'Kullanıcı Yönetimi' })).toBeVisible();
  });

  test('navigates to the reports page', async ({ page }) => {
    await page.getByRole('link', { name: 'Raporlar' }).click();
    await expect(page).toHaveURL('http://localhost:3000/reports');
    await expect(page.getByRole('heading', { name: 'Raporlar ve İstatistikler' })).toBeVisible();
  });

  test('navigates to the settings page', async ({ page }) => {
    await page.getByRole('link', { name: 'Ayarlar' }).click();
    await expect(page).toHaveURL('http://localhost:3000/settings');
    await expect(page.getByRole('heading', { name: 'Ayarlar', exact: true })).toBeVisible();
  });
});
