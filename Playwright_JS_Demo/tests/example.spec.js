//Based on https://www.rabbitmq.com/tutorials/tutorial-one-javascript

// @ts-check
import { test, expect } from '@playwright/test';

let castService;

try {
  const { default: CAST_Client_Service } = await import('../CAST_Client_Service.js');
  castService = new CAST_Client_Service();
  await castService.startService();

  await castService.updateFrameworkFunctionality(
    true,      // startEnabled
    true,      // stopEnabled
    true,      // pauseEnabled
    true,      // resumeEnabled
    true,      // abortEnabled
    false,     // restartEnabled
    true,      // uploadResultEnabled
    'Playwright Javascript Framework',     // frameworkName
    'TestGroup',        // filterOnGroup
    'TestOwner',        // filterOnOwner
    'TestLocation'    // filterOnLocation
  );

  await castService.updateState('Running tests', 'green');
} catch (error) {
  console.warn('CAST Client Service initialization failed:', error.message);
}



test('has title', async ({ page }) => {
  await page.goto('https://playwright.dev/');

  // Expect a title "to contain" a substring.
  await expect(page).toHaveTitle(/Playwright/);
});

test('get started link', async ({ page }) => {
  await page.goto('https://playwright.dev/');

  // Click the get started link.
  await page.getByRole('link', { name: 'Get started' }).click();

  // Expects page to have a heading with the name of Installation.
  await expect(page.getByRole('heading', { name: 'Installation' })).toBeVisible();
});
