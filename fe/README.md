# Simplified Cloud Job Frontend

Minimal React + Tailwind + axios UI for the take-home assignment.

## Stack

- React 19
- Vite
- TypeScript
- Tailwind CSS 4
- axios

## API contract used

- `POST /api/jobs` - submit a job with multipart form data
- `GET /api/jobs/{jobId}` - fetch job status/detail
- `POST /api/jobs/{id}/complete` - simulate EC2 completion
- `GET /api/jobs/billing-summary` - list jobs and billing data

## Local setup

1. Install dependencies from this folder:

   ```bash
   npm install
   ```

2. Create a `.env` file if needed:

   ```bash
   VITE_API_BASE_URL=http://localhost:5175
   ```

3. Start the app:

   ```bash
   npm run dev
   ```

4. Build for production:

   ```bash
   npm run build
   ```

## Notes

- The job list is sourced from billing summary because the backend does not expose a separate list endpoint.
- The submit form uploads a real input file, matching the backend multipart request.
- The complete-job panel can be used to simulate the EC2 callback with an output file and execution duration.