import { test as base } from '@playwright/test';

// Define the types for our fixtures
type MyFixtures = {
  mockApi: (path: string, response: any) => Promise<void>;
  seedData: (endpoint: string, data: any) => Promise<void>;
};

// Extend base test with custom fixtures
export const test = base.extend<MyFixtures>({
  mockApi: async ({ page }, use) => {
    // Custom fixture to mock API responses
    const mockApi = async (path: string, response: any) => {
      await page.route(`**/api/${path}`, async (route) => {
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(response),
        });
      });
    };
    await use(mockApi);
  },

  seedData: async ({ request }, use) => {
    // Helper to seed test data via API
    const seedData = async (endpoint: string, data: any) => {
      const response = await request.post(`/api/seed/${endpoint}`, {
        data,
      });
      if (!response.ok()) {
        throw new Error(`Failed to seed data: ${response.statusText()}`);
      }
    };
    await use(seedData);
  },
});

export { expect } from '@playwright/test';
